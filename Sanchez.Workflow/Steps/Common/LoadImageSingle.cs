using Sanchez.Processing.Models.Projections;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using Sanchez.Workflow.Models.Steps;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Common;

internal sealed class LoadImageSingle : StepBodyAsync, IRegistrationStepBody
{
    public Registration? Registration { get; set; }

    public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        // Get output filename
        ArgumentNullException.ThrowIfNull(Registration);

        // Load image
        await Registration.LoadAsync();
        return ExecutionResult.Next();
    }
}

internal static class LoadImageExtensions
{
    internal static IStepBuilder<TData, LoadImageSingle> LoadImageSingle<TData>(this IWorkflowBuilder<TData> builder)
        where TData : WorkflowData
        => builder
            .StartWith<LoadImageSingle, TData>("Load image")
            .WithRegistration();
}