using System;
using System.Linq.Expressions;
using Ardalis.GuardClauses;
using Sanchez.Processing.Extensions.Images;
using Sanchez.Processing.ImageProcessing.Clut;
using Sanchez.Processing.Models;
using Sanchez.Processing.Services;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Common
{
    public class RenderOverlay : StepBody
    {
        private readonly IClutService _clutService;
        private readonly RenderOptions _options;

        public Image<Rgba32>? SourceImage { get; set; }
        public Image<Rgba32>? OverlayImage { get; set; }

        public RenderOverlay(IClutService clutService, RenderOptions options)
        {
            _clutService = clutService;
            _options = options;
        }

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            var overlayOptions = _options.Overlay;

            if (!overlayOptions.ApplyOverlay) return ExecutionResult.Next();
            Guard.Against.Null(SourceImage, nameof(SourceImage));
            var clut = _clutService.GetClut();

            var equalisedSource = SourceImage.Clone();
            equalisedSource.AdjustLevels(_options.AdaptiveLevelAdjustment);

            OverlayImage = new Image<Rgba32>(equalisedSource.Width, equalisedSource.Height);

            var operation = new ApplyClutRowOperation(equalisedSource, OverlayImage, clut);
            ParallelRowIterator.IterateRows(Configuration.Default, SourceImage.Bounds(), in operation);

            return ExecutionResult.Next();
        }
    }

    internal static class RenderOverlayExtensions
    {
        /// <summary>
        ///     Renders an overlay on a registration image.
        /// </summary>
        internal static IStepBuilder<TData, RenderOverlay> RenderOverlay<TStep, TData>(this IStepBuilder<TData, TStep> builder)
            where TStep : IStepBody
            where TData : WorkflowData
            => builder
                .Then<TStep, RenderOverlay, TData>("Render overlay")
                .Input(step => step.SourceImage, data => data.Registration!.Image)
                .Output(data => data.OverlayImage, step => step.OverlayImage);

        internal static IStepBuilder<TData, RenderOverlay> RenderOverlay<TStep, TData>(
            this IStepBuilder<TData, TStep> builder, Expression<Func<TData, Image<Rgba32>?>>? image)
            where TStep : IStepBody
            where TData : WorkflowData
            => builder
                .Then<TStep, RenderOverlay, TData>("Render overlay")
                .Input(step => step.SourceImage, image)
                .Output(data => data.OverlayImage, step => step.OverlayImage);
    }
}