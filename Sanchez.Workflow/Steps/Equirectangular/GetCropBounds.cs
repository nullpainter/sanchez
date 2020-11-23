using System;
using System.Linq;
using Ardalis.GuardClauses;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Sanchez.Processing.Extensions;
using Sanchez.Processing.Models;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Equirectangular
{
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
            Guard.Against.Null(TargetImage, nameof(TargetImage));
            Guard.Against.Null(Activity, nameof(Activity));

            var autoCrop = _options.EquirectangularRender?.AutoCrop ?? false;
            var extents = _options.EquirectangularRender?.Extents;

            // TODO document this
            // TODO create .bat test files for full earth coverage (and integration tests)
            if (!FullEarthCoverage)
            {
                var minXLongitude = Activity.Registrations
                    .Where(r => r.LongitudeRange != null && !r.LongitudeRange.OverlappingLeft)
                    .Min(r => r.LongitudeRange!.Range.Start)
                    .NormaliseLongitude();

                var minX = minXLongitude.ToX(TargetImage.Width);

                var maxXLongitude = Activity.Registrations
                    .Where(r => r.LongitudeRange != null && !r.LongitudeRange.OverlappingRight)
                    .Max(r => r.LongitudeRange!.Range.End)
                    .NormaliseLongitude();

                var maxX = maxXLongitude.ToX(TargetImage.Width);

                var uncoveredX = minX < maxX ? maxX - minX : TargetImage.Width - (minX - maxX);
                CropBounds = new Rectangle(0, 0, uncoveredX, TargetImage.Height);
            }
            else CropBounds = TargetImage.Bounds();

            if (!autoCrop && extents == null) return ExecutionResult.Next();

            if (autoCrop)
            {
                if (FullEarthCoverage)
                {
                    var croppedLength = (int) Math.Round(AutoCropGlobalScaleFactor * TargetImage.Width);
                    CropBounds = new Rectangle(0, croppedLength, TargetImage.Width, TargetImage.Height - croppedLength * 2);
                }
                else
                {
                    var croppedLength = (int) Math.Round(AutoCropScaleFactor * TargetImage.Width);
                    CropBounds = Rectangle.Inflate(CropBounds, -croppedLength, -croppedLength);
                }

                _logger.LogInformation("Cropped image size: {width} x {height} px", CropBounds.Width, CropBounds.Height);
            }
            else
            {
                var xPixelRange = extents!.Value.Longitude.UnwrapLongitude().ToPixelRangeX(TargetImage.Width);
                var yPixelRange = extents!.Value.Latitude.ToPixelRangeY(TargetImage.Height);

                _logger.LogInformation("Cropped image size: {width} x {height} px", xPixelRange.Range, yPixelRange.Range);

                CropBounds = new Rectangle(0, xPixelRange.Range, yPixelRange.Start, yPixelRange.Range);
            }

            return ExecutionResult.Next();
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
                .Input(step => step.GlobalOffset, data => data.GlobalOffset)
                .Output(data => data.CropBounds, step => step.CropBounds);
        }
    }
}