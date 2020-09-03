using System;
using System.Threading.Tasks;
using Funhouse.ImageProcessing.Offset;
using Funhouse.ImageProcessing.Underlay;
using Funhouse.Models;
using Funhouse.Models.Angles;
using Funhouse.Models.Configuration;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Range = Funhouse.Models.Angles.Range;

namespace Funhouse.Services.Underlay
{
    public interface IUnderlayService
    {
        Task<Image<Rgba32>> GetUnderlayAsync(UnderlayProjectionOptions options, SatelliteDefinition? definition = null, string? underlayPath = null);
    }

    public class UnderlayService : IUnderlayService
    {
        private readonly IUnderlayCache _cache;
        private readonly RenderOptions _options;

        public UnderlayService(IUnderlayCache cache, RenderOptions options)
        {
            _cache = cache;
            _options = options;
        }

        public async Task<Image<Rgba32>> GetUnderlayAsync(UnderlayProjectionOptions options, SatelliteDefinition? definition = null, string? underlayPath = null)
        {
            // TODO this should also cover pixel ranges so we do the cropping, longitude range etc. in one go

            var cached = await _cache.GetUnderlayAsync(definition, options, underlayPath);
            if (cached != null) return cached;

            // Load master equirectangular underlay from disk
            var underlay = await Image.LoadAsync<Rgba32>(underlayPath ?? Constants.DefaultUnderlayPath);
            var target = GetProjected(underlay, definition, options);
            Resize(options, target);

            await _cache.SetUnderlayAsync(target, definition, options, underlayPath);
            return target;
        }

        private static void Resize(UnderlayProjectionOptions options, Image<Rgba32> underlay)
        {
            var targetSize = options.TargetSize;
            if (targetSize == null) return;
            
            // Resize underlay to target image size
            Log.Information("Resizing underlay to {width} x {height} px", targetSize.Value.Width, targetSize.Value.Height);

            underlay.Mutate(c => c.Resize(targetSize.Value.Width, targetSize.Value.Height));
        }

        private Image<Rgba32> GetProjected(Image<Rgba32> underlay, SatelliteDefinition? definition, UnderlayProjectionOptions options)
        {
            switch (options.Projection)
            {
                case ProjectionType.Geostationary:
                    if (definition == null) throw new InvalidOperationException("Satellite definition must be provided for geostationary projection");

                    // Project underlay to geostationary, based on the target satellite
                    Log.Information("{definition:l0} Rendering geostationary underlay", definition.DisplayName);
                    return underlay.ToGeostationaryProjection(definition, _options);
                case ProjectionType.Equirectangular:

                    // Optionally crop to specified lat/long range
                    if (options.CropSpecified)
                    {
                        Crop(underlay, options.LatitudeCrop!.Value, options.LongitudeCrop!.Value);
                    }

                    return underlay;
                default:
                    throw new ArgumentOutOfRangeException($"Unhandled projection type: {options.Projection}");
            }
        }

        /// <summary>
        ///     Crops an equirectangular underlay to a specified latitude and longitude range. 
        /// </summary>
        private void Crop(Image<Rgba32> underlay, Range latitudeRange, Range longitudeRange)
        {
            var xPixelRange = PixelRange.ToPixelRangeX(longitudeRange, underlay.Width);
            var yPixelRange = PixelRange.ToPixelRangeY(latitudeRange, underlay.Height);

            Log.Information("Cropping underlay to {min} - {max} px width", xPixelRange.Start, xPixelRange.End);
            Log.Information("Cropping underlay to {min} - {max} px height", yPixelRange.Start, yPixelRange.End);

            // Offset and wrap underlay horizontally if required to match projection
            if (xPixelRange.End > underlay.Width)
            {
                var offset = -xPixelRange.Start;
                Log.Information("Offsetting underlay by {pixels} px", offset);
                underlay.HorizontalOffset(offset);

                xPixelRange = new PixelRange(0, xPixelRange.Range);
            }

            // Crop underlay
            underlay.Mutate(c => c.Crop(new Rectangle(xPixelRange.Start, yPixelRange.Start, xPixelRange.Range, yPixelRange.Range)));
        }
    }
}