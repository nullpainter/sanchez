using System;
using System.IO;
using System.Threading.Tasks;
using Dapper;
using Sanchez.Helpers;
using Sanchez.Models;
using Sanchez.Models.Configuration;
using Sanchez.Seeder;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using Serilog;

namespace Sanchez.Services.Underlay
{
    /// <summary>
    ///     Underlying storage mechanism for the underlay cache.
    /// </summary>
    public interface IUnderlayCacheRepository
    {
        /// <summary>
        ///     Performs database initialisation.
        /// </summary>
        void Initialise();
        
        /// <summary>
        ///     Adds an underlay registration to the database.
        /// </summary>
        /// <param name="definition">optional satellite definition used to generate underlay</param>
        /// <param name="options">underlay rendering options</param>
        /// <param name="filename">filename of rendered underlay</param>
        Task RegisterCacheAsync(SatelliteDefinition? definition, ProjectionOptions options, string filename);
        
        /// <summary>
        ///     Removes an underlay registration from the database.
        /// </summary>
        /// <param name="filename">filename of rendered underlay</param>
        Task ClearCacheEntryAsync(string filename);
        
        /// <summary>
        ///     Retrieves a cached underlay filename based on an optional satellite definition and render options, or
        ///     <c>null</c> if the underlay isn't in the cache.
        /// </summary>
        Task<string?> GetCacheFilenameAsync(SatelliteDefinition? definition, ProjectionOptions options);

        /// <summary>
        ///     Deletes the underlay cache database.
        /// </summary>
        void DeleteCache();
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
            try
            {
                // Ensure database is up-to-date
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

        /// <summary>
        ///     Deletes the underlay cache database.
        /// </summary> 
        public void DeleteCache() => File.Delete(CacheDatabase);

        /// <summary>
        ///     Adds an underlay registration to the database.
        /// </summary>
        /// <param name="definition">optional satellite definition used to generate underlay</param>
        /// <param name="options">underlay rendering options</param>
        /// <param name="filename">filename of rendered underlay</param>
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

        /// <summary>
        ///     Removes an underlay registration from the database.
        /// </summary>
        /// <param name="filename">filename of rendered underlay</param>
        public async Task ClearCacheEntryAsync(string filename)
        {
            await using var connection = new SqliteConnection(ConnectionString);
            await connection.OpenAsync();
            
            await connection.ExecuteAsync("DELETE FROM UnderlayCache where Filename=@Filename", new
            {
                Filename = filename
            });
        }

        /// <summary>
        ///     Retrieves a cached underlay filename based on an optional satellite definition and render options, or
        ///     <c>null</c> if the underlay isn't in the cache.
        /// </summary>
        public async Task<string?> GetCacheFilenameAsync(SatelliteDefinition? definition, ProjectionOptions options)
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