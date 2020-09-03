using System;
using System.IO;
using System.Threading.Tasks;
using Dapper;
using Funhouse.Helpers;
using Funhouse.Models;
using Funhouse.Models.Configuration;
using Funhouse.Seeder;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using Serilog;

namespace Funhouse.Services.Underlay
{
    public interface IUnderlayCacheRepository
    {
        void Initialise();
        Task RegisterCacheAsync(SatelliteDefinition? definition, ProjectionOptions options, string filename);
        Task ClearCacheEntryAsync(string filename);
        Task<string> GetCacheFilenameAsync(SatelliteDefinition? definition, ProjectionOptions options);
    }

    public class UnderlayCacheRepository : IUnderlayCacheRepository
    {
        private readonly string _cachePath;
        private readonly IDatabaseMigrator _databaseMigrator;

        private string CacheDatabase => Path.Combine(_cachePath, "cache.db");
        private string ConnectionString => $"Data Source={CacheDatabase}";
        
        public UnderlayCacheRepository(IDatabaseMigrator databaseMigrator)
        {
            _databaseMigrator = databaseMigrator;

            _cachePath = PathHelper.CachePath();
            if (!Directory.Exists(_cachePath)) Directory.CreateDirectory(_cachePath);
        }


        public void Initialise()
        {
            // Ensure database is up-to-date
            try
            {
                _databaseMigrator.Migrate(ConnectionString);
            }
            catch (Exception)
            {
                Log.Error("Error performing database migration; deleting database and retrying");

                // Delete database and try to migrate again, in case the database file is corrupt or 
                // there was an incompatible schema update.
                File.Delete(CacheDatabase);
                _databaseMigrator.Migrate(ConnectionString);
            }
        }

        public async Task RegisterCacheAsync(SatelliteDefinition? definition, ProjectionOptions options, string filename)
        {
            await using var connection = new SqliteConnection(ConnectionString);
            await connection.OpenAsync();

            string sql = definition == null
                ? "INSERT INTO UnderlayCache(Filename, Configuration) VALUES(@Filename, @Configuration)"
                : "INSERT INTO UnderlayCache(Filename, Configuration, Longitude) VALUES(@Filename, @Configuration, @Longitude)";

            await connection.ExecuteAsync(sql, new
            {
                Filename = filename,
                Configuration = JsonConvert.SerializeObject(options),
                definition?.Longitude
            });
        }

        public async Task ClearCacheEntryAsync(string filename)
        {
            await using var connection = new SqliteConnection(ConnectionString);
            await connection.OpenAsync();
            
            await connection.ExecuteAsync("DELETE FROM UnderlayCache where Filename=@Filename", new
            {
                Filename = filename
            });
        }

        public async Task<string> GetCacheFilenameAsync(SatelliteDefinition? definition, ProjectionOptions options)
        {
            await using var connection = new SqliteConnection(ConnectionString);
            await connection.OpenAsync();
           
            var sql = "SELECT Filename FROM UnderlayCache WHERE Configuration=@Configuration";
            if (definition != null) sql += " AND Longitude=@Longitude";

            return await connection.QueryFirstOrDefaultAsync<string>(sql, new
            {
                definition?.Longitude,
                Configuration = JsonConvert.SerializeObject(options)
            });
        }
    }
}