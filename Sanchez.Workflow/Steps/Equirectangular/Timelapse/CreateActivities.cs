using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Projections;
using Sanchez.Processing.Services;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using Sanchez.Workflow.Models.Steps;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Equirectangular.Timelapse;

internal sealed class CreateActivities(
    ILogger<CreateActivities> logger,
    ISatelliteImageLoader loader) : StepBody, IActivityStepBody
{
    public Activity? Activity { get; set; }
    internal DateTime? Timestamp { get; [UsedImplicitly] set; }
    internal int NumTimeIntervals { get; [UsedImplicitly] set; }
    public List<Registration>? SourceRegistrations { get; set; }

    public override ExecutionResult Run(IStepExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(Timestamp);
        ArgumentNullException.ThrowIfNull(SourceRegistrations);

        // Load images
        logger.LogInformation("Loading source images for timestamp {Timestamp}", Timestamp);
        Activity = loader.RegisterImages(SourceRegistrations, Timestamp);

        return ExecutionResult.Next();
    }
}

internal static class CreateActivitiesExtensions
{
    internal static IStepBuilder<TData, CreateActivities> CreateActivities<TStep, TData>(this IStepBuilder<TData, TStep> builder)
        where TStep : IStepBody
        where TData : TimelapseWorkflowData
        => builder
            .Then<TStep, CreateActivities, TData>("Create activities")
            .Input(step => step.SourceRegistrations, data => data.SourceRegistrations)
            .Input(step => step.Timestamp, data => data.TargetTimestamp)
            .Input(step => step.NumTimeIntervals, data => data.TimeIntervals.Count)
            .Output(data => data.Activity, step => step.Activity);
}