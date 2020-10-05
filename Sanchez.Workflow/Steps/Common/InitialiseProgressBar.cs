using System;
using System.Linq.Expressions;
using Ardalis.GuardClauses;
using Sanchez.Processing.Helpers;
using Sanchez.Processing.Models;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models;
using Sanchez.Workflow.Models.Data;
using ShellProgressBar;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Common
{
    internal class InitialiseProgressBar : StepBody
    {
        private readonly RenderOptions _options;
        internal IProgressBar? ProgressBar { get; private set; }
        public int MaxTicks { get; set; }

        public InitialiseProgressBar(RenderOptions options) => _options = options;

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            Guard.Against.Null(MaxTicks, nameof(MaxTicks));

            ProgressBar = ProgressBarFactory.NewProgressBar(_options, MaxTicks);
            return ExecutionResult.Next();
        }
    }

    internal static class InitialiseProgressBarExtensions
    {
        internal static IStepBuilder<TData, InitialiseProgressBar> InitialiseProgressBar<TData>(this IWorkflowBuilder<TData> builder, Expression<Func<TData, int>> maxTicks)
            where TData : WorkflowData
            => builder
                .StartWith<InitialiseProgressBar, TData>()
                .Input(step => step.MaxTicks, maxTicks)
                .Output(data => data.ProgressBar, step => step.ProgressBar);

        internal static IStepBuilder<TData, InitialiseProgressBar> InitialiseProgressBar<TStep, TData>(this IStepBuilder<TData, TStep> builder, Expression<Func<TData, int>> maxTicks)
            where TStep : IStepBody
            where TData : WorkflowData
            => builder
                .Then<TStep, InitialiseProgressBar, TData>()
                .Input(step => step.MaxTicks, maxTicks)
                .Output(data => data.ProgressBar, step => step.ProgressBar);
    }
}