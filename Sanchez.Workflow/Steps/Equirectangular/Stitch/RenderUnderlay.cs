using System.IO;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Extend;
using Microsoft.Extensions.Logging;
using Sanchez.Processing.Extensions.Images;
using Sanchez.Processing.ImageProcessing.Tint;
using Sanchez.Processing.Models;
using Sanchez.Processing.Services.Underlay;
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
        internal Image<Rgba32>? TargetImage { get; set;  }
        
        public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            Guard.Against.Null(Activity, nameof(Activity));
            Guard.Against.Null(TargetImage, nameof(TargetImage));

            // Determine visible range of all satellite imagery
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
                clone.Mutate(cloneContext => cloneContext.HistogramEqualization());
                TargetImage.Tint(_options.Tint);

                imageContext.DrawImage(clone, PixelColorBlendingMode.HardLight, 0.5f);
            });
            
            var underlayOptions = new UnderlayProjectionData(
                ProjectionType.Equirectangular,
                _options.InterpolationType,
                _options.ImageSize,
                TargetImage!.Height,
                latitudeRange,
                longitudeRange);

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
                .Output(data => data.TargetImage, step => step.TargetImage);
        }
    }
}