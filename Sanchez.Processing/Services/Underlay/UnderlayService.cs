using Microsoft.Extensions.Logging;
using Sanchez.Processing.Extensions;
using Sanchez.Processing.ImageProcessing.Underlay;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Angles;
using Sanchez.Processing.Models.Configuration;
using Sanchez.Processing.Models.Projections;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Sanchez.Processing.Services.Underlay;

public interface IUnderlayService
{
    /// <summary>
    ///     Retrieves a full-colour underlay image with the target projection. Underlays are cached to disk to speed up
    ///     computation.
    /// </summary>
    /// <param name="data">Underlay generation options</param>
    /// <param name="definition">Optional satellite definition, if projecting underlay to match a satellite IR image</param>
    /// <param name="ct"></param>
    /// <returns>projected underlay</returns>
    Task<Image<Rgba32>> GetUnderlayAsync(UnderlayProjectionData data, SatelliteDefinition? definition, CancellationToken ct = default);
}

public class UnderlayService : IUnderlayService
{
    private readonly ILogger<UnderlayService> _logger;
    private readonly IUnderlayCache _cache;
    private readonly RenderOptions _options;

    public UnderlayService(
        ILogger<UnderlayService> logger,
        IUnderlayCache cache,
        RenderOptions options)
    {
        _logger = logger;
        _cache = cache;
        _options = options;
    }

    /// <summary>
    ///     Retrieves a full-colour underlay image with the target projection. Underlays are cached to disk to speed up
    ///     computation.
    /// </summary>
    /// <param name="data">Underlay generation options</param>
    /// <param name="definition">Optional satellite definition, if projecting underlay to match a satellite IR image</param>
    /// <param name="ct"></param>
    /// <returns>projected underlay</returns>
    public async Task<Image<Rgba32>> GetUnderlayAsync(UnderlayProjectionData data, SatelliteDefinition? definition, CancellationToken ct = default)
    {
        // Attempt to retrieve underlay from cache
        var cached = await _cache.GetUnderlayAsync(definition, data);
        if (cached != null) return cached;

        // Load master equirectangular underlay image from disk
        var underlay = await Image.LoadAsync<Rgba32>(_options.UnderlayPath, ct);

        // Project to match satellite imagery
        var target = GetProjected(underlay, definition, data);
        Resize(data, target);

        // Register underlay in cache
        await _cache.SetUnderlayAsync(target, definition, data);
        return target;
    }

    /// <summary>
    ///     Optionally resizes the underlay based on the target height.
    /// </summary>
    private void Resize(UnderlayProjectionData data, Image<Rgba32> underlay)
    {
        if (data.TargetSize == null) return;

        // Resize underlay to target image size
        underlay.Mutate(c => c.Resize(data.TargetSize.Value));
    }

    /// <summary>
    ///     Returns an underlay projected and optionally cropped.
    /// </summary>
    private Image<Rgba32> GetProjected(Image<Rgba32> underlay, SatelliteDefinition? definition, UnderlayProjectionData data)
    {
        switch (data.Projection)
        {
            case ProjectionType.Geostationary:
                if (definition == null) throw new InvalidOperationException("Satellite definition must be provided for geostationary projection");

                // Project underlay to geostationary, based on the target satellite
                _logger.LogInformation("{Definition:l0} Rendering geostationary underlay", definition.DisplayName);
               
                var geostationary = underlay.ToGeostationaryProjection(definition.Longitude, definition.Height, _options);
                
                // Set black background
                var image = new Image<Rgba32>(geostationary.Width, geostationary.Height);
                image.Mutate(c => c.BackgroundColor(Color.Black));

                image.Mutate(c => c.DrawImage(geostationary, PixelColorBlendingMode.Normal, 1.0f));
                return image;

            case ProjectionType.Equirectangular:

                // Perform latitude crop to match IR imagery if required
                var equirectangularOptions = _options.EquirectangularRender;

                if (equirectangularOptions == null || !equirectangularOptions.NoCrop && !equirectangularOptions.ExplicitCrop)
                {
                    Crop(underlay, data.LatitudeCrop!.Value);
                }

                return underlay;
            default:
                throw new ArgumentOutOfRangeException($"Unhandled projection type: {data.Projection}");
        }
    }

    /// <summary>
    ///     Crop to a specified latitude range.
    /// </summary>
    private void Crop(Image<Rgba32> underlay, AngleRange latitudeRange)
    {
        var yPixelRange = latitudeRange.ToPixelRangeY(underlay.Height);

        // Crop underlay to target height
        _logger.LogInformation("Cropping underlay to {Min} - {Max} px height", yPixelRange.Start, yPixelRange.End);
        underlay.Mutate(c => c.Crop(new Rectangle(0, yPixelRange.Start, underlay.Width, yPixelRange.Range)));
    }
}