using System;
using Ardalis.GuardClauses;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Sanchez.Processing.Extensions;
using Sanchez.Processing.ImageProcessing.Offset;
using Sanchez.Processing.Models;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using Sanchez.Workflow.Models.Steps;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Equirectangular
{
    internal sealed class CropImage : StepBody, IActivityStepBody
    {
        private readonly RenderOptions _options;
        private readonly ILogger<CropImage> _logger;
        public Image<Rgba32>? TargetImage { get; [UsedImplicitly] set; }
        public Rectangle? CropBounds { get; [UsedImplicitly] set; }

        public Activity? Activity { get; set; }
        public double GlobalOffset { get; [UsedImplicitly] set; }

        public CropImage(RenderOptions options, ILogger<CropImage> logger)
        {
            _options = options;
            _logger = logger;
        }

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            Guard.Against.Null(TargetImage, nameof(TargetImage));
            Guard.Against.Null(Activity, nameof(Activity));

            // TODO comment and move; isn't related to cropping per-se
            if (!Activity.IsFullEarthCoverage())
            {
                var offset = (GlobalOffset - Math.PI)
                    .NormaliseLongitude()
                    .ToX(TargetImage.Width);

                TargetImage.HorizontalOffset(offset);
            }

            // Crop if required
            var equirectangularOptions = _options.EquirectangularRender!;
            if (!equirectangularOptions.NoCrop && CropBounds != null)
            {
                _logger.LogDebug("Crop bounds: {bounds}", CropBounds);
                TargetImage.Mutate(c => c.DrawLines(Color.Red, 2,
                    new PointF(CropBounds.Value.X, CropBounds.Value.Y),
                    new PointF(CropBounds.Value.X, CropBounds.Value.Y + CropBounds.Value.Height),
                    new PointF(CropBounds.Value.X + CropBounds.Value.Width, CropBounds.Value.Y + CropBounds.Value.Height),
                    new PointF(CropBounds.Value.X + CropBounds.Value.Width, CropBounds.Value.Y),
                    new PointF(CropBounds.Value.X, CropBounds.Value.Y)
                ));
                // TargetImage.Mutate(ctx => ctx.Crop(CropBounds.Value));
            }

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
                .Input(step => step.GlobalOffset, data => data.GlobalOffset)
                .Input(step => step.CropBounds, data => data.CropBounds)
                .Input(step => step.Activity, data => data.Activity);
        }
    }
}