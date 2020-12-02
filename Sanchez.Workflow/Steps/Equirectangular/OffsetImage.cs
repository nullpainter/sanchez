using System;
using Ardalis.GuardClauses;
using JetBrains.Annotations;
using Sanchez.Processing.Extensions;
using Sanchez.Processing.ImageProcessing.Offset;
using Sanchez.Processing.Models;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using Sanchez.Workflow.Models.Steps;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Equirectangular
{
    // TODO comment
    internal sealed class OffsetImage : StepBody, IActivityStepBody
    {
        private readonly RenderOptions _options;
        public Activity? Activity { get; [UsedImplicitly] set; }
        public Image<Rgba32>? TargetImage { get; [UsedImplicitly] set; }
        public double GlobalOffset { get; [UsedImplicitly] set; }

        public OffsetImage(RenderOptions options) => _options = options;

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            Guard.Against.Null(TargetImage, nameof(TargetImage));
            Guard.Against.Null(Activity, nameof(Activity));

            // TODO create test case for this
            if (Activity.IsFullEarthCoverage() && !_options.EquirectangularRender!.ExplicitCrop) return ExecutionResult.Next();
            
            var offset = GlobalOffset
                .NormaliseLongitude()
                .ToX(TargetImage.Width);
            
            TargetImage.HorizontalOffset(offset);
            
            return ExecutionResult.Next();
        }
    }
    
    internal static class OffsetImageExtensions
    {
        internal static IStepBuilder<TData, OffsetImage> OffsetImage<TStep, TData>(this IStepBuilder<TData, TStep> builder)
            where TStep : IStepBody
            where TData : StitchWorkflowData
        {
            return builder
                .Then<TStep, OffsetImage, TData>("Offset image")
                .WithActivity()
                .Input(step => step.TargetImage, data => data.TargetImage)
                .Input(step => step.GlobalOffset, data => data.GlobalOffset)
                .Input(step => step.Activity, data => data.Activity);
        }
    }
}