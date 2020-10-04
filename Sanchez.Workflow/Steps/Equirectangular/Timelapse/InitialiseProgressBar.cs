using System;
using System.Linq.Expressions;
using Ardalis.GuardClauses;
using JetBrains.Annotations;
using Sanchez.Processing.Helpers;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models;
using ShellProgressBar;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Equirectangular.Timelapse
{
    internal class InitialiseImageProgressBar : StepBody, IProgressBarStepBody
    {
        public IProgressBar? ProgressBar { get; [UsedImplicitly] set; }
        internal IProgressBar? ImageProgressBar { get; [UsedImplicitly] set; }
        public int MaxTicks { get; set; }

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            Guard.Against.Null(MaxTicks, nameof(MaxTicks));
            Guard.Against.Null(ProgressBar, nameof(ProgressBar));

            var options = ProgressBarFactory.DefaultOptions();
            options.ForegroundColor = ConsoleColor.Blue;

            ImageProgressBar ??= ProgressBar.Spawn(MaxTicks, null, options);
            
            ImageProgressBar.Tick(0);
            ImageProgressBar.MaxTicks = MaxTicks;

            return ExecutionResult.Next();
        }
    }

    internal static class InitialiseImageProgressBarExtensions
    {
        internal static IStepBuilder<TData, InitialiseImageProgressBar> InitialiseImageProgressBar<TData>(this IWorkflowBuilder<TData> builder, Expression<Func<TData, int>> maxTicks)
            where TData : EquirectangularTimelapseWorkflowData
            => builder
                .StartWith<InitialiseImageProgressBar, TData>()
                .WithProgressBar()
                .Input(step => step.ImageProgressBar, data => data.ImageProgressBar)
                .Input(step => step.MaxTicks, maxTicks)
                .Output(data => data.ImageProgressBar, step => step.ImageProgressBar);
    }
}