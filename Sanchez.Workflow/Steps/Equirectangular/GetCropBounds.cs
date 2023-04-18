using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Sanchez.Processing.Extensions;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Angles;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Equirectangular;

internal sealed class GetCropBounds : StepBody
{
    private readonly ILogger<GetCropBounds> _logger;
    private readonly RenderOptions _options;

    /// <summary>
    ///     Proportion of image to be cropped when performing auto crop.
    /// </summary>
    private const float AutoCropScaleFactor = 0.05f;

    private const float AutoCropGlobalScaleFactor = 0.02f;

    public GetCropBounds(RenderOptions options, ILogger<GetCropBounds> logger)
    {
        _options = options;
        _logger = logger;
    }

    internal Rectangle CropBounds { get; private set; }

    public Image<Rgba32>? TargetImage { get; [UsedImplicitly] set; }
    public bool FullEarthCoverage { get; [UsedImplicitly] set; }
    public Activity? Activity { get; [UsedImplicitly] set; }
    public double GlobalOffset { get; [UsedImplicitly] set; }

    public override ExecutionResult Run(IStepExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(TargetImage);
        ArgumentNullException.ThrowIfNull(Activity);

        var autoCrop = _options.EquirectangularRender?.AutoCrop ?? false;
        var explicitCrop = _options.EquirectangularRender?.ExplicitCrop ?? false;


        // TODO document all of this - entire class really
        // Also, refactor. It's a mess.
        if (!autoCrop && !explicitCrop)
        {
            CropBounds = !FullEarthCoverage ? GetPartialCoverageBounds(Activity, TargetImage) : TargetImage.Bounds;
            return ExecutionResult.Next();
        }

        CropBounds = autoCrop ? GetAutoCropBounds(TargetImage) : GetExplicitCropBounds(TargetImage);

        _logger.LogInformation("Cropped image size: {Width} x {Height} px", CropBounds.Width, CropBounds.Height);

        return ExecutionResult.Next();
    }

    private Rectangle GetExplicitCropBounds(Image targetImage)
    {
        var latitudeRange = _options.EquirectangularRender!.LatitudeRange;
        var longitudeRange = _options.EquirectangularRender!.LongitudeRange;

        // Underlay is being offset by the global offset, so we need to add it back to get the 
        // correct x pixel range for crop.
        var xPixelRange = (longitudeRange + GlobalOffset - Math.PI)?
            .UnwrapLongitude()
            .ToPixelRangeX(targetImage.Width) ?? new PixelRange(0, targetImage.Width);

        var yPixelRange = latitudeRange != null
            ? latitudeRange!.Value
                .ToPixelRangeY(targetImage.Height)
            : new PixelRange(0, targetImage.Height);

        _logger.LogDebug("Crop bounds: [ X={XBounds}, Y={YBounds} ]", xPixelRange, yPixelRange);
        return new Rectangle(xPixelRange.Start, yPixelRange.Start, xPixelRange.Range, yPixelRange.Range);
    }

    private Rectangle GetAutoCropBounds(Image targetImage)
    {
        if (FullEarthCoverage)
        {
            var croppedLength = (int) Math.Round(AutoCropGlobalScaleFactor * targetImage.Width);
            return new Rectangle(0, croppedLength, targetImage.Width, targetImage.Height - croppedLength * 2);
        }
        else
        {
            var partialCoverageBounds = GetPartialCoverageBounds(Activity!, targetImage);
                
            var croppedLength = (int) Math.Round(AutoCropScaleFactor * targetImage.Width);
            return Rectangle.Inflate(partialCoverageBounds, -croppedLength, -croppedLength);
        }
    }

    private Rectangle GetPartialCoverageBounds(Activity activity, Image targetImage)
    {
        var minXLongitude = activity.Registrations
            .Where(r => r.LongitudeRange is { OverlappingLeft: false })
            .Min(r => r.LongitudeRange!.Value.Range.Start)
            .NormaliseLongitude();

        var minX = minXLongitude.ToX(targetImage.Width);

        var maxXLongitude = activity.Registrations
            .Where(r => r.LongitudeRange is { OverlappingRight: false })
            .Max(r => r.LongitudeRange!.Value.Range.End)
            .NormaliseLongitude();

        var maxX = maxXLongitude.ToX(targetImage.Width);

        var uncoveredX = minX < maxX ? maxX - minX : targetImage.Width - (minX - maxX);
        return new Rectangle(0, 0, uncoveredX, targetImage.Height);
    }
}

internal static class GetCropBoundsExtensions
{
    internal static IStepBuilder<TData, GetCropBounds> GetCropBounds<TStep, TData>(this IStepBuilder<TData, TStep> builder)
        where TStep : IStepBody
        where TData : StitchWorkflowData
    {
        return builder
            .Then<TStep, GetCropBounds, TData>("Get crop bounds")
            .Input(step => step.FullEarthCoverage, data => data.Activity!.IsFullEarthCoverage())
            .Input(step => step.TargetImage, data => data.TargetImage)
            .Input(step => step.Activity, data => data.Activity)
            .Input(data => data.GlobalOffset, step => step.GlobalOffset)
            .Output(data => data.CropBounds, step => step.CropBounds);
    }
}