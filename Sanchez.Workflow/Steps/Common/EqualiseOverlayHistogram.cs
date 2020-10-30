using System;
using System.Linq.Expressions;
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
    public class EqualiseOverlayHistogram : StepBody
    {
        private readonly RenderOptions _options;

        public EqualiseOverlayHistogram(RenderOptions options) => _options = options;
        public Image<Rgba32>? Image { get; set; }
        
        public override ExecutionResult Run(IStepExecutionContext context)
        {
            if (!_options.Overlay.ApplyOverlay) return ExecutionResult.Next();
            
            Guard.Against.Null(Image, nameof(Image));
            Image.Mutate(c => c.HistogramEqualization());
            return ExecutionResult.Next();
        }
    }
    
    internal static class EqualiseHistogramExtensions
    {
        internal static IStepBuilder<TData, EqualiseOverlayHistogram> EqualiseOverlayHistogram<TStep, TData>(this IStepBuilder<TData, TStep> builder)
            where TStep : IStepBody
            where TData : WorkflowData
            => builder
                .Then<TStep, EqualiseOverlayHistogram, TData>("Render overlay")
                .Input(step => step.Image, data => data.Registration!.Image);

        internal static IStepBuilder<TData, EqualiseOverlayHistogram> EqualiseOverlayHistogram<TData, TStep>(
            this IStepBuilder<TData, TStep> builder, Expression<Func<TData, Image<Rgba32>?>>? image)
                    where TStep : IStepBody
            where TData : WorkflowData
            => builder
                .Then<TStep, EqualiseOverlayHistogram, TData>("Render overlay")
                .Input(step => step.Image, image);
    }
}