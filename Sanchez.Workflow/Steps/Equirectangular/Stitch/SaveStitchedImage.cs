using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using JetBrains.Annotations;
using Sanchez.Processing.Extensions.Images;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Projections;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models;
using Sanchez.Workflow.Models.Data;
using Sanchez.Workflow.Models.Steps;
using ShellProgressBar;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Equirectangular.Stitch
{
    internal sealed class SaveStitchedImage : StepBodyAsync, IActivityStepBody, IProgressBarStepBody
    {
        public int RenderedCount { get; [UsedImplicitly] set; }
        public IProgressBar? ProgressBar { get; [UsedImplicitly] set; } = null!;

        public Image<Rgba32>? TargetImage { get; set; }
        public Activity? Activity { get; set; }

        public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            Guard.Against.Null(TargetImage, nameof(TargetImage));
            Guard.Against.Null(ProgressBar, nameof(ProgressBar));
            Guard.Against.Null(Activity?.OutputPath, nameof(Registration.OutputPath));

            ProgressBar.Tick("Saving image");

            // Save image
            using (TargetImage)
            using (Activity)
            {
                await TargetImage!.SaveWithExifAsync(Activity.OutputPath);
            }

            RenderedCount++;

            return ExecutionResult.Next();
        }
    }

    internal static class SaveImageExtensions
    {
        internal static IStepBuilder<TData, SaveStitchedImage> SaveStitchedImage<TStep, TData>(this IStepBuilder<TData, TStep> builder, Expression<Func<TData, IProgressBar?>> progressBar)
            where TStep : IStepBody
            where TData : EquirectangularStitchWorkflowData
        {
            return builder
                .Then<TStep, SaveStitchedImage, TData>("Save image")
                .WithActivity()
                .Input(step => step.ProgressBar, progressBar)
                .Input(step => step.RenderedCount, data => data.RenderedCount)
                .Input(step => step.TargetImage, data => data.TargetImage)
                .Output(data => data.RenderedCount, step => step.RenderedCount);
        }
    }
}