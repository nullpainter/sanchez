using Microsoft.Extensions.Logging;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Projections;
using Sanchez.Processing.Services;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Common;

internal sealed class CreateActivity(
    RenderOptions options,
    ILogger<CreateActivity> logger,
    ISatelliteImageLoader loader) : StepBody
{
    internal Activity? Activity { get; private set; }
    public List<Registration>? SourceRegistrations { get; set; }

    public override ExecutionResult Run(IStepExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(SourceRegistrations);

        // Load images
        logger.LogInformation("Loading source images");
        Activity = loader.RegisterImages(SourceRegistrations, options.Timestamp);

        return ExecutionResult.Next();
    }
}

internal static class CreateActivityExtensions
{
    internal static IStepBuilder<TData, CreateActivity> CreateActivity<TStep, TData>(this IStepBuilder<TData, TStep> builder)
        where TStep : IStepBody
        where TData : WorkflowData
        => builder
            .Then<TStep, CreateActivity, TData>("Create activity")
            .Input(step => step.SourceRegistrations, data => data.SourceRegistrations)
            .Output(data => data.Activity, step => step.Activity);
}