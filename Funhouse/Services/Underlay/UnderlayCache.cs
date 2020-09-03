using System;
using System.IO;
using System.Threading.Tasks;
using Funhouse.Helpers;
using Funhouse.Models;
using Funhouse.Models.Configuration;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Funhouse.Services.Underlay
{
    
    public interface IUnderlayCache
    {
        Task<Image<Rgba32>?> GetUnderlayAsync(SatelliteDefinition? definition, ProjectionOptions options, string? underlayPath);
        Task SetUnderlayAsync(Image<Rgba32> underlay, SatelliteDefinition? definition, ProjectionOptions options, string? underlayPath);
    }
    
    public class UnderlayCache : IUnderlayCache
    {
        private readonly IUnderlayCacheRepository _repository;
        private readonly string _cachePath;

        public UnderlayCache(IUnderlayCacheRepository repository)
        {
            _repository = repository;
               
            _cachePath = PathHelper.CachePath();
            if (!Directory.Exists(_cachePath)) Directory.CreateDirectory(_cachePath);
            
        }

        public async Task<Image<Rgba32>?> GetUnderlayAsync(SatelliteDefinition? definition, ProjectionOptions options, string? underlayPath)
        {
            var path = await _repository.GetCacheFilenameAsync(definition, options);
            if (path == null) return null;
            
            var fullPath = Path.Combine(_cachePath, path);

            if (!File.Exists(fullPath))
            {
                Log.Warning($"Cache file {path} not found; removing from cache registration", path);
                await _repository.ClearCacheEntryAsync(path);
            }

            try
            {
                if (definition == null) Log.Information("Using cached underlay");
                else Log.Information("{definition:l0} Using cached underlay", definition.DisplayName);
                
                return await Image.LoadAsync<Rgba32>(fullPath);
            }
            catch (Exception e)
            {
                Log.Warning(e, $"Cache file {path} unable to be read; removing from cache registration", path);
                await _repository.ClearCacheEntryAsync(path);
            }

            return null;
        }

        public async Task SetUnderlayAsync(Image<Rgba32> underlay, SatelliteDefinition? definition, ProjectionOptions options, string? underlayPath)
        {
            if (definition == null) Log.Information("Caching underlay");
            else Log.Information("{definition:l0} Caching underlay", definition.DisplayName);
            
            var filename = $"{Guid.NewGuid()}.jpg";
            await underlay.SaveAsync(Path.Combine(_cachePath, filename));
            
            await _repository.RegisterCacheAsync(definition, options, filename);
        }

    }
}