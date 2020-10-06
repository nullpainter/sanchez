using Ardalis.GuardClauses;
using JetBrains.Annotations;
using Sanchez.Processing.Extensions;
using Sanchez.Processing.Models;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using Sanchez.Workflow.Models.Steps;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Equirectangular.Stitch
{
    internal sealed class CropImage : StepBody, IActivityStepBody
    {
        private readonly RenderOptions _options;

        public CropImage(RenderOptions options) => _options = options;

        public Image<Rgba32>? TargetImage { get; [UsedImplicitly] set; }
        public Rectangle? CropBounds { get; [UsedImplicitly] set; }

        public Activity? Activity { get; set; }

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            Guard.Against.Null(TargetImage, nameof(TargetImage));

            if (!_options.NoUnderlay)
            {
                Guard.Against.Null(Activity, nameof(Activity));
                var xPixelRange = Activity.GetVisibleLongitudeRange().UnwrapLongitude().ToPixelRangeX(TargetImage.Width);
                if (xPixelRange.Range > 0) TargetImage.Mutate(c => c.Crop(new Rectangle(0, 0, xPixelRange.Range, c.GetCurrentSize().Height)));
            }

            if (CropBounds != null) TargetImage.Mutate(ctx => ctx.Crop(CropBounds.Value));
            return ExecutionResult.Next();
        }
    }

    internal static class CropImageExtensions
    {
        internal static IStepBuilder<TData, CropImage> CropImage<TStep, TData>(this IStepBuilder<TData, TStep> builder)
            where TStep : IStepBody
            where TData : StitchWorkflowData
        {
            return builder
                .Then<TStep, CropImage, TData>("Crop image")
                .WithActivity()
                .Input(step => step.TargetImage, data => data.TargetImage)
                .Input(step => step.CropBounds, data => data.CropBounds);
        }
    }
}