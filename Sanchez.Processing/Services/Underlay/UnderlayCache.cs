using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sanchez.Processing.Helpers;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Sanchez.Processing.Services.Underlay
{
    /// <summary>
    ///     Cache for rendered underlay images.
    /// </summary>
    public interface IUnderlayCache
    {
        /// <summary>
        ///     Retrieves an underlay image from the cache.
        /// </summary>
        Task<Image<Rgba32>?> GetUnderlayAsync(SatelliteDefinition? definition, ProjectionOptions options);
        
        /// <summary>
        ///     Sets an underlay image to the cache.
        /// </summary>
        Task SetUnderlayAsync(Image<Rgba32> underlay, SatelliteDefinition? definition, ProjectionOptions options);
    }
    
    public class UnderlayCache : IUnderlayCache
    {
        private readonly IUnderlayCacheRepository _repository;
        private readonly string _cachePath;
        private readonly ILogger<UnderlayCache> _logger;

        public UnderlayCache(IUnderlayCacheRepository repository, ILogger<UnderlayCache> logger)
        {
            _repository = repository;
            _logger = logger;

            // Initialise cache path if required
            _cachePath = PathHelper.CachePath();
            if (!Directory.Exists(_cachePath)) Directory.CreateDirectory(_cachePath);
            
        }

        /// <summary>
        ///     Retrieves an underlay image from the cache.
        /// </summary>
        public async Task<Image<Rgba32>?> GetUnderlayAsync(SatelliteDefinition? definition, ProjectionOptions options)
        {
            var path = await _repository.GetCacheFilenameAsync(definition, options);
            if (path == null) return null;
            
            var fullPath = Path.Combine(_cachePath, path);

            // Remove underlay registration from cache if it has been deleted from disk
            if (!File.Exists(fullPath))
            {
                _logger.LogWarning("Cache file {path} not found; removing from cache registration", path);
                await _repository.ClearCacheEntryAsync(path);
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
        public async Task SetUnderlayAsync(Image<Rgba32> underlay, SatelliteDefinition? definition, ProjectionOptions options)
        {
            if (definition == null) _logger.LogInformation("Caching underlay");
            else _logger.LogInformation("{definition:l0} Caching underlay", definition.DisplayName);
            
            // Save underlay to disk
            var filename = $"{Guid.NewGuid()}.jpg";
            await underlay.SaveAsync(Path.Combine(_cachePath, filename));
            
            // Register underlay path in the cache
            await _repository.RegisterCacheAsync(definition, options, filename);
        }
    }
}