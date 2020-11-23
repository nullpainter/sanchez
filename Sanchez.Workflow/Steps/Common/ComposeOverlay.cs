using Ardalis.GuardClauses;
using Sanchez.Processing.Models;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Common
{
    public class ComposeOverlay : StepBody
    {
        private readonly RenderOptions _options;
        public Image<Rgba32>? OverlayImage { get; set; }
        public Image<Rgba32>? TargetImage { get; set; }

        public ComposeOverlay(RenderOptions options) => _options = options;

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            if (!_options.Overlay.ApplyOverlay) return ExecutionResult.Next();

            Guard.Against.Null(OverlayImage, nameof(OverlayImage));
            Guard.Against.Null(TargetImage, nameof(TargetImage));

            TargetImage.Mutate(c => c.DrawImage(OverlayImage, PixelColorBlendingMode.Normal, 1.0f));
            
            return ExecutionResult.Next();
        }
    }

    internal static class ComposeOverlayExtensions
    {
        /// <summary>
        ///     Renders an overlay on a registration image.
        /// </summary>
        internal static IStepBuilder<TData, ComposeOverlay> ComposeOverlay<TStep, TData>(this IStepBuilder<TData, TStep> builder)
            where TStep : IStepBody
            where TData : WorkflowData
            => builder
                .Then<TStep, ComposeOverlay, TData>("Compose overlay")
                .Input(step => step.TargetImage, data => data.TargetImage)
                .Input(step => step.OverlayImage, data => data.OverlayImage);
    }
}