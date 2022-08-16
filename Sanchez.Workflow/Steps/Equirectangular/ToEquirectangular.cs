using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Sanchez.Processing.Extensions;
using Sanchez.Processing.ImageProcessing.Projection;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Angles;
using Sanchez.Processing.Models.Projections;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using Sanchez.Workflow.Models.Steps;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Equirectangular;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
internal sealed class ToEquirectangular : StepBody, IRegistrationStepBody
{
    private readonly ILogger<ToEquirectangular> _logger;
    private readonly RenderOptions _options;

    public ToEquirectangular(
        ILogger<ToEquirectangular> logger,
        RenderOptions options)
    {
        _logger = logger;
        _options = options;
    }

    public Image<Rgba32>? SourceImage { get; set; }
    public Image<Rgba32>? TargetImage { get; set; }
    public Registration? Registration { get; set; }

    public override ExecutionResult Run(IStepExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(Registration?.Definition);
        ArgumentNullException.ThrowIfNull(SourceImage);

        // Reproject geostationary image into equirectangular
        LogStatistics();
        TargetImage = Reproject(Registration);

        // Overlap range relative the satellite's visible range and convert to a equirectangular map
        // offset with a pixel range of -180 to 180 degrees.
        var longitude = Registration.Definition.LongitudeRange.Start.NormaliseLongitude();
        Registration.OffsetX = longitude.ScaleToWidth(_options.ImageSize * 2);

        return ExecutionResult.Next();
    }

    private Image<Rgba32> Reproject(Registration registration)
    {
        var definition = registration.Definition;

        // Preserve 2:1 equirectangular aspect ratio
        var maxWidth = _options.ImageSize * 2;
        var maxHeight = _options.ImageSize;

        // Determine pixel ranges of projected image so we can limit our processing to longitudes visible to satellite
        // Unwrap the longitude range to simplify maths
        var longitudeRange = new AngleRange(
            definition.LongitudeRange.Start,
            definition.LongitudeRange.End).UnwrapLongitude();

        var latitudeRange = new AngleRange(definition.LatitudeRange.Start, definition.LatitudeRange.End);

        _logger.LogInformation("{Definition:l0} latitude range {StartRange:F2} to {EndRange:F2} degrees",
            definition.DisplayName,
            Angle.FromRadians(latitudeRange.Start).Degrees,
            Angle.FromRadians(latitudeRange.End).Degrees);

        _logger.LogInformation("{Definition:l0} unwrapped longitude range {StartRange:F2} to {EndRange:F2} degrees",
            definition.DisplayName,
            Angle.FromRadians(longitudeRange.Start).Degrees,
            Angle.FromRadians(longitudeRange.End).Degrees);

        // Get size of projection in pixels
        var xRange = new PixelRange(longitudeRange, a => a.ScaleToWidth(maxWidth));

        // Restrict height of image to the visible range if we are not performing explicit cropping or no cropping
        var yRange = _options.EquirectangularRender?.NoCrop == true || _options.EquirectangularRender?.ExplicitCrop == true
            ? new PixelRange(0, maxHeight)
            : new PixelRange(latitudeRange, a => a.ScaleToHeight(maxHeight));

        _logger.LogInformation("{Definition:l0} pixel range X: {MinX} - {MaxX} px", definition.DisplayName, xRange.Start, xRange.End);
        _logger.LogInformation("{Definition:l0} pixel range Y: {MinY} - {MaxY} px", definition.DisplayName, yRange.Start, yRange.End);

        _logger.LogInformation("{Definition:l0} width: {TargetWidth} px", definition.DisplayName, xRange.Range);
        _logger.LogInformation("{Definition:l0} height: {TargetWidth} px", definition.DisplayName, yRange.Range);

        // Create target image with the correct dimensions for the projected satellite image
        var target = new Image<Rgba32>(xRange.Range, yRange.Range);
        _logger.LogInformation("{Definition:l0} Reprojecting", definition.DisplayName);

        // Perform reprojection
        var operation = new ReprojectRowOperation(registration, SourceImage!, target, xRange.Start, yRange.Start, _options);
        target.Mutate(c => c.ProcessPixelRowsAsVector4((row, point) => operation.Invoke(row, point)));

        return target;
    }

    private void LogStatistics()
    {
        var definition = Registration!.Definition;
        var longitudeRange = Registration.LongitudeRange!;

        _logger.LogInformation("{Definition:l0} source image {Path}", definition.DisplayName, Registration.Path);

        _logger.LogInformation("{Definition:l0} range {StartRange:F2} to {EndRange:F2} degrees",
            definition.DisplayName,
            Angle.FromRadians(definition.LongitudeRange.Start).Degrees,
            Angle.FromRadians(definition.LongitudeRange.End).Degrees);

        _logger.LogInformation("{Definition:l0} crop {StartRange:F2} to {EndRange:F2} degrees",
            definition.DisplayName,
            Angle.FromRadians(longitudeRange!.Value.Range.Start).Degrees,
            Angle.FromRadians(longitudeRange.Value.Range.End).Degrees);
    }
}

internal static class ToEquirectangularExtensions
{
    internal static IStepBuilder<TData, ToEquirectangular> ToEquirectangular<TStep, TData>(
        this IStepBuilder<TData, TStep> builder)
        where TStep : IStepBody
        where TData : StitchWorkflowData
        => builder
            .Then<TStep, ToEquirectangular, TData>("Reprojecting to equirectangular")
            .WithRegistration()
            .Input(step => step.SourceImage, data => data.Registration!.Image)
            .Output(data => data.Registration!.Image, step => step.TargetImage);
}