using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Projections;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Equirectangular.Timelapse;

internal class PrepareTimeIntervals : StepBody
{
    private readonly RenderOptions _options;

    public PrepareTimeIntervals(RenderOptions options) => _options = options;

    public List<DateTime> TimeIntervals { get; } = new();
    public List<Registration>? SourceRegistrations { get; set; }

    public override ExecutionResult Run(IStepExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(_options.Interval);
        ArgumentNullException.ThrowIfNull(SourceRegistrations);

        // Either derive the start timestamp from the command-line or from the earliest registered file timestamp
        var registrations = SourceRegistrations.Where(r => r.Timestamp != null).ToList();

        var startTimestamp = _options.Timestamp ??= registrations.Min(r => r.Timestamp)!.Value;
        var endTimestamp = _options.EndTimestamp ??= registrations.Max(r => r.Timestamp)!.Value;

        for (var timestamp = startTimestamp; timestamp < endTimestamp!; timestamp += _options.Interval!.Value)
        {
            TimeIntervals.Add(timestamp);
        }

        return ExecutionResult.Next();
    }
}

internal static class PrepareTimeIntervalsExtensions
{
    internal static IStepBuilder<TData, PrepareTimeIntervals> PrepareTimeIntervals<TStep, TData>(this IStepBuilder<TData, TStep> builder)
        where TStep : IStepBody
        where TData : TimelapseWorkflowData
        => builder
            .Then<TStep, PrepareTimeIntervals, TData>("Prepare time intervals")
            .Input(step => step.SourceRegistrations, data => data.SourceRegistrations)
            .Output(data => data.TimeIntervals, step => step.TimeIntervals);
}