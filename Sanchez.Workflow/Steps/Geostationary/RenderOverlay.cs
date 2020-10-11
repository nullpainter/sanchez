using Ardalis.GuardClauses;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Projections;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using Sanchez.Workflow.Models.Steps;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Geostationary
{
    public class RenderOverlay : StepBody, IRegistrationStepBody
    {
        private readonly RenderOptions _options;
        public Registration? Registration { get; set; }
        internal Image<Rgba32>? TargetImage { get; private set; }
        internal Image<Rgba32>? OverlayImage { get; private set; }

        public RenderOverlay(RenderOptions options) => _options = options;

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            Guard.Against.Null(_options.OverlayPath, nameof(_options.OverlayPath));
            Guard.Against.Null(OverlayImage, nameof(OverlayImage));
            Guard.Against.Null(TargetImage, nameof(TargetImage));
            Guard.Against.Null(Registration, nameof(Registration));

            TargetImage.Mutate(c => c.DrawImage(OverlayImage, PixelColorBlendingMode.Normal, 1.0f));

            return ExecutionResult.Next();
        }
    }

    internal static class RegisterOverlayExtensions
    {
        internal static IStepBuilder<GeostationaryWorkflowData, RenderOverlay> RenderOverlay<TStep>(this IStepBuilder<GeostationaryWorkflowData, TStep> builder)
            where TStep : IStepBody
            => builder
                .Then<TStep, RenderOverlay, GeostationaryWorkflowData>("Render overlay")
                .WithRegistration()
                .Input(step => step.OverlayImage, data => data.OverlayImage)
                .Input(step => step.TargetImage, data => data.TargetImage);
    }
}