using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Equirectangular.Timelapse;

internal sealed class SetTargetTimestamp : StepBody
{
    public override ExecutionResult Run(IStepExecutionContext context)
    {
        var timestamp = (DateTime) context.Item;
        var data = (TimelapseWorkflowData) context.Workflow.Data;

        data.TargetTimestamp = timestamp;
        return ExecutionResult.Next();
    }
}

internal static class SetTargetTimestampExtensions
{
    internal static IStepBuilder<TData, SetTargetTimestamp> SetTargetTimestamp<TData>(this IWorkflowBuilder<TData> builder)
        where TData : WorkflowData
        => builder
            .StartWith<SetTargetTimestamp, TData>();
}