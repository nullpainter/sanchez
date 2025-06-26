using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Projections;
using Sanchez.Processing.Services;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using Sanchez.Workflow.Models.Steps;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Equirectangular;

internal sealed class GetVisibleRange(IProjectionOverlapCalculator projectionOverlapCalculator, RenderOptions options) : StepBody, IActivityStepBody
{
    public Activity? Activity { get; set; }

    public override ExecutionResult Run(IStepExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(Activity);

        // Calculates overlapping regions between satellites, or visible region if not stitching images.
        projectionOverlapCalculator.Initialise(Activity.Registrations.Select(p => p.Definition));

        // Set latitude and longitude ranges based on overlapping satellite ranges
        Activity.Registrations.ForEach(registration =>
        {
            registration.LatitudeRange = new ProjectionRange(registration.Definition.LatitudeRange);
            registration.LongitudeRange =
                options.StitchImages
                    ? projectionOverlapCalculator.GetNonOverlappingRange(registration.Definition)
                    : new ProjectionRange(registration.Definition.LongitudeRange.UnwrapLongitude());
        });

        return ExecutionResult.Next();
    }
}

internal static class GetVisibleRangeExtensions
{
    internal static IStepBuilder<TData, GetVisibleRange> GetVisibleRange<TData>(this IWorkflowBuilder<TData> builder)
        where TData : WorkflowData
        => builder
            .StartWith<GetVisibleRange, TData>("Calculate visible range")
            .WithActivity();

    internal static IStepBuilder<TData, GetVisibleRange> GetVisibleRange<TStep, TData>(this IStepBuilder<TData, TStep> builder)
        where TStep : IStepBody
        where TData : WorkflowData
        => builder
            .Then<TStep, GetVisibleRange, TData>("Calculate visible range")
            .WithActivity();
}