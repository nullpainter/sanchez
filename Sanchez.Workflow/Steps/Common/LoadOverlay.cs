using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Sanchez.Processing.Extensions.Images;
using Sanchez.Processing.ImageProcessing.Mask;
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

namespace Sanchez.Workflow.Steps.Common
{
    public class LoadOverlay : StepBodyAsync, IRegistrationStepBody
    {
        private readonly RenderOptions _options;
        public Registration? Registration { get; set; }
        internal Image<Rgba32>? OverlayImage { get; private set; }

        public LoadOverlay(RenderOptions options) => _options = options;

        public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            Guard.Against.Null(_options.OverlayPath, nameof(_options.OverlayPath));
            Guard.Against.Null(Registration, nameof(Registration));

            var overlay = await Image.LoadAsync<Rgba32>(_options.OverlayPath);
            NormaliseOverlay(overlay);

            OverlayImage = overlay;

            return ExecutionResult.Next();
        }

        private void NormaliseOverlay(Image<Rgba32> overlay)
        {
            if (Registration!.Definition.Crop != null) overlay.CropBorder(Registration.Definition.Crop);
            overlay.Mutate(c => c.Resize(_options.ImageSize, _options.ImageSize));
            overlay.RemoveBackground();
        }
    }

    internal static class LoadOverlayExtensions
    {
        internal static IStepBuilder<TData, LoadOverlay> LoadOverlay<TData>(this IWorkflowBuilder<TData> builder)
            where TData : WorkflowData
            => builder
                .StartWith<LoadOverlay, TData>("Load overlay")
                .WithRegistration()
                .Output(data => data.OverlayImage, step => step.OverlayImage);
    }
}