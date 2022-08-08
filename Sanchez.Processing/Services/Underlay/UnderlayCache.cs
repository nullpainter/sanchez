using Microsoft.Extensions.Logging;
using Sanchez.Processing.Helpers;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Sanchez.Processing.Services.Underlay;

/// <summary>
///     Cache for rendered underlay images.
/// </summary>
public interface IUnderlayCache
{
    /// <summary>
    ///     Retrieves an underlay image from the cache.
    /// </summary>
    Task<Image<Rgba32>?> GetUnderlayAsync(SatelliteDefinition? definition, UnderlayProjectionData data);

    /// <summary>
    ///     Sets an underlay image to the cache.
    /// </summary>
    Task SetUnderlayAsync(Image<Rgba32> underlay, SatelliteDefinition? definition, UnderlayProjectionData data);
}

public class UnderlayCache : IUnderlayCache
{
    private readonly IUnderlayCacheRepository _repository;
    private readonly RenderOptions _options;
    private readonly string _cachePath;
    private readonly ILogger<UnderlayCache> _logger;

    public UnderlayCache(
        IUnderlayCacheRepository repository,
        RenderOptions options,
        ILogger<UnderlayCache> logger)
    {
        _repository = repository;
        _options = options;
        _logger = logger;

        // Initialise cache path if required
        _cachePath = PathHelper.CachePath();
        if (!Directory.Exists(_cachePath)) Directory.CreateDirectory(_cachePath);
    }

    /// <summary>
    ///     Retrieves an underlay image from the cache.
    /// </summary>
    public async Task<Image<Rgba32>?> GetUnderlayAsync(SatelliteDefinition? definition, UnderlayProjectionData data)
    {
        var metadata = await _repository.GetCacheMetadataAsync(definition, data);
        if (metadata == null) return null;

        var path = metadata.Filename;

        var fullPath = Path.Combine(_cachePath, path);

        // Remove underlay registration from cache if it has been deleted from disk
        if (!File.Exists(fullPath))
        {
            _logger.LogWarning("Cache file {path} not found; removing from cache registration", path);
            await _repository.ClearCacheEntryAsync(path);
        }

        var fileTimestamp = File.GetLastWriteTimeUtc(_options.UnderlayPath);
            
        // Check if the source underlay file has been updated since the cache was created
        if (metadata.Timestamp < fileTimestamp)
        {
            _logger.LogInformation("Timestamp of underlay changed; updating cache");
            await _repository.ClearCacheEntryAsync(path);
            return null;
        }

        try
        {
            if (definition == null) _logger.LogInformation("Using cached underlay");
            else _logger.LogInformation("{definition:l0} Using cached underlay", definition.DisplayName);

            return await Image.LoadAsync<Rgba32>(fullPath);
        }
        catch (Exception e)
        {
            // Remove underlay registration from cache if there was an error reading it from disk
            _logger.LogWarning(e, "Cache file {path} unable to be read; removing from cache registration", path);
            await _repository.ClearCacheEntryAsync(path);
        }

        return null;
    }

    /// <summary>
    ///     Sets an underlay image to the cache.
    /// </summary>
    public async Task SetUnderlayAsync(Image<Rgba32> underlay, SatelliteDefinition? definition, UnderlayProjectionData data)
    {
        if (definition == null) _logger.LogInformation("Caching underlay");
        else _logger.LogInformation("{definition:l0} Caching underlay", definition.DisplayName);

        // Save underlay to disk
        var filename = $"{Guid.NewGuid()}.jpg";
        var underlayPath = Path.Combine(_cachePath, filename);
        await underlay.SaveAsync(underlayPath);

        // Register underlay path in the cache
        await _repository.RegisterCacheAsync(definition, data, underlayPath);
    }
}