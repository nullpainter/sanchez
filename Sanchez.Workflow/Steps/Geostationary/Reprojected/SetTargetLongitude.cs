using Sanchez.Processing.Extensions;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Angles;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Geostationary.Reprojected;

internal class SetTargetLongitude : StepBody
{
    private readonly RenderOptions _options;

    public SetTargetLongitude(RenderOptions options) => _options = options;
    public double? Longitude { get; set; }

    /// <summary>
    ///     Timestamp of currently-processed step.
    /// </summary>
    public DateTime? TargetTimestamp { get; set; }

    public List<DateTime>? TimeIntervals { get; set; }

    public override ExecutionResult Run(IStepExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(TargetTimestamp);

        var geostationaryOptions = _options.GeostationaryRender!;

        Longitude = geostationaryOptions.EndLongitude == null
            ? geostationaryOptions.Longitude
            : GetTimelapseLongitude(geostationaryOptions);

        return ExecutionResult.Next();
    }

    private double GetTimelapseLongitude(GeostationaryRenderOptions geostationaryOptions)
    {
        ArgumentNullException.ThrowIfNull(TimeIntervals);
        if (TimeIntervals.Count == 0) throw new ArgumentException(nameof(TimeIntervals));

        var inverse = geostationaryOptions.InverseRotation;
            
        var rawIndex = TimeIntervals.IndexOf(TargetTimestamp!.Value);
            
        // Note that the default start and end longitude are inverted by default to simplify maths
        var currentIndex = inverse ? rawIndex : TimeIntervals.Count - 1 - rawIndex;
        if (currentIndex < 0) throw new InvalidOperationException($"Unable to find timestamp {TargetTimestamp} in timelapse");

        var end = inverse ? geostationaryOptions.EndLongitude!.Value : geostationaryOptions.Longitude!.Value;
        var start = inverse ? geostationaryOptions.Longitude!.Value : geostationaryOptions.EndLongitude!.Value;

        var range = new AngleRange(start, end).UnwrapLongitude();
        var offset = TimeIntervals.Count == 1 ? 0 : (range.End - range.Start) * (currentIndex / ((double) TimeIntervals.Count - 1));

        return (start + offset).NormaliseLongitude();
    }
}

public static class SetTargetLongitudeExtensions
{
    internal static IStepBuilder<TData, SetTargetLongitude> SetTargetLongitude<TStep, TData>(this IStepBuilder<TData, TStep> builder)
        where TStep : IStepBody
        where TData : GeostationaryTimelapseWorkflowData
        => builder
            .Then<TStep, SetTargetLongitude, TData>()
            .Input(step => step.TargetTimestamp, data => data.TargetTimestamp)
            .Input(step => step.TimeIntervals, data => data.TimeIntervals)
            .Output(data => data.Longitude, step => step.Longitude);
}