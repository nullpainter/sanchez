using Microsoft.Extensions.Logging;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Projections;
using Sanchez.Processing.Services;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Equirectangular;

internal sealed class RegisterImages(
    RenderOptions options,
    ILogger<RegisterImages> logger,
    ISatelliteImageLoader loader) : StepBody
{
    public List<Registration>? SourceRegistrations { get; set; }
    internal List<Activity>? Activities { get; private set; }

    public override ExecutionResult Run(IStepExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(SourceRegistrations);

        logger.LogInformation("Loading source images");

        var masterActivity = loader.RegisterImages(SourceRegistrations, options.Timestamp);
        Activities = masterActivity.Registrations.Select(registration => new Activity(new List<Registration> { registration })).ToList();

        return ExecutionResult.Next();
    }
}

internal static class RegisterImagesExtensions
{
    internal static IStepBuilder<EquirectangularWorkflowData, RegisterImages> RegisterImages<TStep>(this IStepBuilder<EquirectangularWorkflowData, TStep> builder)
        where TStep : IStepBody
        => builder
            .Then<TStep, RegisterImages, EquirectangularWorkflowData>("Register images")
            .Input(step => step.SourceRegistrations, data => data.SourceRegistrations)
            .Output(data => data.Activities, step => step.Activities);
}