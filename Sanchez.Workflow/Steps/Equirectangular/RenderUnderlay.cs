using System.IO;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Extend;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Sanchez.Processing.Extensions.Images;
using Sanchez.Processing.ImageProcessing.Tint;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Projections;
using Sanchez.Processing.Services.Underlay;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using Sanchez.Workflow.Models.Steps;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Equirectangular
{
    public sealed class RenderUnderlay : StepBodyAsync, IActivityStepBody
    {
        private readonly RenderOptions _options;
        private readonly IUnderlayService _underlayService;
        private readonly ILogger<RenderUnderlay> _logger;

        public RenderUnderlay(
            RenderOptions options,
            IUnderlayService underlayService,
            ILogger<RenderUnderlay> logger)
        {
            _options = options;
            _underlayService = underlayService;
            _logger = logger;
        }
        
        public Activity? Activity { get; set; }
        internal Image<Rgba32>? TargetImage { get; [UsedImplicitly] set; }
        public Rectangle? CropBounds { get; [UsedImplicitly] set; }

        public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            Guard.Against.Null(Activity, nameof(Activity));
            Guard.Against.Null(TargetImage, nameof(TargetImage));

            Activity.GetCropRange(out var latitudeRange, out var longitudeRange);
            
            // Load underlay
            if (_options.NoUnderlay)
            {
                // Draw stitched image over black background for non-PNG files because otherwise alpha can look odd
                TargetImage = !Path.GetExtension(_options.OutputPath).CompareOrdinalIgnoreCase(".png") ? TargetImage.AddBackgroundColour(Color.Black) : TargetImage;
                return ExecutionResult.Next();
            }

            _logger.LogInformation("Tinting and normalising IR imagery");

            TargetImage.Mutate(imageContext =>
            {
                using var clone = TargetImage!.Clone();
                clone.AdjustLevels();
                TargetImage.Tint(_options.Tint);

                imageContext.DrawImage(clone, PixelColorBlendingMode.HardLight, 0.5f);
            });
            
            var underlayOptions = new UnderlayProjectionData(
                ProjectionType.Equirectangular,
                _options.InterpolationType,
                _options.UnderlayPath,
                _options.ImageSize,
                TargetImage!.Size(),
                latitudeRange,
                longitudeRange.Start,
                _options.EquirectangularRender?.NoCrop == true || _options.EquirectangularRender?.ExplicitCrop == true);

            _logger.LogInformation("Retrieving underlay");
            var underlay = await _underlayService.GetUnderlayAsync(underlayOptions);

            // Render target image onto underlay
            using (TargetImage)
            {
                _logger.LogInformation("Blending with underlay");
                underlay.Mutate(ctx => ctx.DrawImage(TargetImage, PixelColorBlendingMode.Screen, 1.0f));
            }
            
            TargetImage = underlay;

            return ExecutionResult.Next();
        }
    }
    
    internal static class RenderUnderlayExtensions
    {
        internal static IStepBuilder<TData, RenderUnderlay> RenderUnderlay<TStep, TData>(this IStepBuilder<TData, TStep> builder)
            where TStep : IStepBody
            where TData : StitchWorkflowData
        {
            return builder
                .Then<TStep, RenderUnderlay, TData>("Render underlay")
                .WithActivity()
                .Input(step => step.TargetImage, data => data.TargetImage)
                .Input(step => step.CropBounds, data => data.CropBounds)
                .Output(data => data.TargetImage, step => step.TargetImage);
        }
    }
}