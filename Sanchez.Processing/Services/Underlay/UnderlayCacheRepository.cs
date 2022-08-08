using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sanchez.Processing.Helpers;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Configuration;
using Sanchez.Processing.Services.Database;

namespace Sanchez.Processing.Services.Underlay;

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
    /// <param name="data">underlay rendering options</param>
    /// <param name="underlayPath">path to rendered underlay</param>
    Task RegisterCacheAsync(SatelliteDefinition? definition, UnderlayProjectionData data, string underlayPath);
        
    /// <summary>
    ///     Removes an underlay registration from the database.
    /// </summary>
    /// <param name="filename">filename of rendered underlay</param>
    Task ClearCacheEntryAsync(string filename);
        
    /// <summary>
    ///     Retrieves a cached underlay filename and timestamp based on an optional satellite definition and render options, or
    ///     <c>null</c> if the underlay isn't in the cache.
    /// </summary>
    Task<UnderlayMetadata?> GetCacheMetadataAsync(SatelliteDefinition? definition, UnderlayProjectionData data);

    /// <summary>
    ///     Deletes the underlay cache database.
    /// </summary>
    void DeleteCache();
}

public class UnderlayCacheRepository : IUnderlayCacheRepository
{
    private readonly string _cachePath;
    private readonly IDatabaseMigrator _databaseMigrator;
    private readonly ILogger<UnderlayCacheRepository> _logger;

    private string CacheDatabase => Path.Combine(_cachePath, "cache.db");
    private string ConnectionString => $"Data Source={CacheDatabase}";
        
    public UnderlayCacheRepository(
        ILogger<UnderlayCacheRepository> logger,
        IDatabaseMigrator databaseMigrator)
    {
        _logger = logger;
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
            _logger.LogError("Error performing database migration; deleting database and retrying");

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
    /// <param name="data">underlay rendering options</param>
    /// <param name="underlayPath">path to rendered underlay</param>
    public async Task RegisterCacheAsync(SatelliteDefinition? definition, UnderlayProjectionData data, string underlayPath)
    {
        await using var connection = new SqliteConnection(ConnectionString);
        await connection.OpenAsync();

        string sql = definition == null
            ? "INSERT INTO UnderlayCache(Filename, Configuration, Timestamp) VALUES(@Filename, @Configuration, @Timestamp)"
            : "INSERT INTO UnderlayCache(Filename, Configuration, Longitude, Timestamp) VALUES(@Filename, @Configuration, @Longitude, @Timestamp)";

        var timestamp = File.GetLastWriteTimeUtc(underlayPath);
        await connection.ExecuteAsync(sql, new
        {
            Filename = underlayPath,
            Configuration = JsonConvert.SerializeObject(data),
            Longitude = definition?.Longitude,
            Timestamp = (DateTimeOffset)timestamp
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
    ///     Retrieves a cached underlay filename and timestamp based on an optional satellite definition and render options, or
    ///     <c>null</c> if the underlay isn't in the cache.
    /// </summary>
    public async Task<UnderlayMetadata?> GetCacheMetadataAsync(SatelliteDefinition? definition, UnderlayProjectionData data)
    {
        await using var connection = new SqliteConnection(ConnectionString);
        await connection.OpenAsync();

        var sql = "SELECT Filename, Timestamp FROM UnderlayCache WHERE Configuration=@Configuration";
        if (definition != null) sql += " AND Longitude=@Longitude";

        return await connection.QueryFirstOrDefaultAsync<UnderlayMetadata>(sql, new
        {
            definition?.Longitude,
            Configuration = JsonConvert.SerializeObject(data)
        });
    }
}