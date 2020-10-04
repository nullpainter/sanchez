using Ardalis.GuardClauses;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models;
using ShellProgressBar;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Common
{
    internal sealed class LogCompletion : StepBody, IProgressBarStepBody
    {
        private readonly ILogger<LogCompletion> _logger;

        public LogCompletion(ILogger<LogCompletion> logger) => _logger = logger;
        
        public int RenderedCount { get; [UsedImplicitly] set; }
        public int AlreadyRenderedCount { get; [UsedImplicitly] set; }
        
        public IProgressBar? ProgressBar { get; set; }

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            Guard.Against.Null(ProgressBar, nameof(ProgressBar));
            
            var message = $"Completed rendering {RenderedCount} {(RenderedCount == 1 ? "image" : "images")}";
            if (AlreadyRenderedCount > 0) message += $"; skipped {AlreadyRenderedCount} {(AlreadyRenderedCount == 1 ? "image" : "images")}";

            ProgressBar.Tick(message);
            _logger.LogInformation(message);

            return ExecutionResult.Next();
        }
    }

    internal static class LogCompletionExtensions
    {
        internal static IStepBuilder<TData, LogCompletion> LogCompletion<TData>(this IWorkflowBuilder<TData> builder)
            where TData : WorkflowData
            => builder
                .StartWith<LogCompletion, TData>()
                .WithProgressBar()
                .Input(step => step.RenderedCount, data => data.RenderedCount)
                .Input(step => step.AlreadyRenderedCount, data => data.AlreadyRenderedCount);

        internal static IStepBuilder<TData, LogCompletion> LogCompletion<TStep, TData>(this IStepBuilder<TData, TStep> builder)
            where TStep : IStepBody
            where TData : WorkflowData
            => builder
                .Then<TStep, LogCompletion, TData>()
                .WithProgressBar()
                .Input(step => step.RenderedCount, data => data.RenderedCount)
                .Input(step => step.AlreadyRenderedCount, data => data.AlreadyRenderedCount);
    }
}