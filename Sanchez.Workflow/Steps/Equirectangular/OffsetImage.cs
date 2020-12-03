using Ardalis.GuardClauses;
using JetBrains.Annotations;
using Sanchez.Processing.Extensions;
using Sanchez.Processing.ImageProcessing.Offset;
using Sanchez.Processing.Models;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using Sanchez.Workflow.Models.Steps;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Equirectangular
{
    /// <summary>
    ///     Horizontally offsets and wraps an equirectangular projected image by <see cref="GlobalOffset"/> radians. This is
    ///     performed to ensure that visible satellite imagery is contiguous in the rendered image.
    /// </summary>
    internal sealed class OffsetImage : StepBody, IActivityStepBody
    {
        private readonly RenderOptions _options;
        public Activity? Activity { get; [UsedImplicitly] set; }
        public Image<Rgba32>? TargetImage { get; [UsedImplicitly] set; }
        public double GlobalOffset { get; [UsedImplicitly] set; }

        public OffsetImage(RenderOptions options) => _options = options;

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            Guard.Against.Null(TargetImage, nameof(TargetImage));
            Guard.Against.Null(Activity, nameof(Activity));

            var offset = GetOffset(Activity);
            if (offset == null) return ExecutionResult.Next();
            
            var pixelOffset = offset.Value
                .NormaliseLongitude()
                .ToX(TargetImage.Width);

            TargetImage.HorizontalOffset(pixelOffset);

            return ExecutionResult.Next();
        }

        /// <summary>
        ///     Returns offset in radians to apply to image, or <c>null</c> if no offset is to be performed.
        /// </summary>
        private double? GetOffset(Activity activity)
        {
            var equirectangularOptions = _options.EquirectangularRender!;

            // No offset required if full earth coverage and no explicit crop is being performed
            return activity!.IsFullEarthCoverage() && !equirectangularOptions.ExplicitCrop 
                ? equirectangularOptions.StartLongitude?.Radians 
                : GlobalOffset;
        }
    }

    internal static class OffsetImageExtensions
    {
        /// <summary>
        ///     Horizontally offsets and wraps an equirectangular projected image by <see cref="StitchWorkflowData.GlobalOffset"/> radians. 
        /// </summary>
        internal static IStepBuilder<TData, OffsetImage> OffsetImage<TStep, TData>(this IStepBuilder<TData, TStep> builder)
            where TStep : IStepBody
            where TData : StitchWorkflowData
        {
            return builder
                .Then<TStep, OffsetImage, TData>("Offset image")
                .WithActivity()
                .Input(step => step.TargetImage, data => data.TargetImage)
                .Input(step => step.GlobalOffset, data => data.GlobalOffset)
                .Input(step => step.Activity, data => data.Activity);
        }
    }
}