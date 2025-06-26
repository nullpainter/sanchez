using Sanchez.Processing.Services.Underlay;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Common;

internal class InitialiseUnderlayCache(IUnderlayCacheRepository cacheRepository) : StepBody
{
    public override ExecutionResult Run(IStepExecutionContext context)
    {
        cacheRepository.Initialise();
        return ExecutionResult.Next();
    }
}

internal static class InitialiseUnderlayCacheExtensions
{
    internal static IStepBuilder<TData, InitialiseUnderlayCache> InitialiseUnderlayCache<TData>(this IWorkflowBuilder<TData> builder)
        where TData : WorkflowData
        => builder
            .StartWith<InitialiseUnderlayCache, TData>("Initialise underlay cache");
}