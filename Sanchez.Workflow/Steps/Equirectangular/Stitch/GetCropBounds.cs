using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Sanchez.Processing.Extensions;
using Sanchez.Processing.Extensions.Images;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Projections;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models;
using Sanchez.Workflow.Models.Data;
using Sanchez.Workflow.Models.Steps;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Equirectangular.Stitch
{
    internal sealed class GetCropBounds : StepBody, IRegistrationStepBody
    {
        private readonly ILogger<GetCropBounds> _logger;
        private readonly RenderOptions _options;

        public GetCropBounds(RenderOptions options, ILogger<GetCropBounds> logger)
        {
            _options = options;
            _logger = logger;
        }

        public Image<Rgba32>? TargetImage { get; set; }
        public Registration? Registration { get; set; }
        internal Rectangle? CropBounds { get; private set; }

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            Guard.Against.Null(TargetImage, nameof(TargetImage));

            var autoCrop = _options.EquirectangularRender?.AutoCrop ?? false;
            var extents = _options.EquirectangularRender?.Extents;

            if (!autoCrop && extents == null) return ExecutionResult.Next();

            if (autoCrop)
            {
                CropBounds = TargetImage.GetAutoCropBounds();

                if (CropBounds == null) _logger.LogError("Unable to determine autocrop bounds");
                else _logger.LogInformation("Cropped image size: {width} x {height} px", CropBounds.Value.Width, CropBounds.Value.Height);
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
            where TData : EquirectangularStitchWorkflowData
        {
            return builder
                .Then<TStep, GetCropBounds, TData>("Get crop bounds")
                .Input(step => step.TargetImage, data => data.TargetImage)
                .Output(data => data.CropBounds, step => step.CropBounds);
        }
    }
}