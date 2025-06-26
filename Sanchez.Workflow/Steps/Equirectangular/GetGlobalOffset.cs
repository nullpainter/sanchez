using Microsoft.Extensions.Logging;
using Sanchez.Processing.Extensions;
using Sanchez.Processing.Models;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using Sanchez.Workflow.Models.Steps;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Equirectangular;

// TODO we need to test this!
/// <summary>
///     Calculates the horizontal offset to be applied to stitched IR images so the images don't
///     wrap around the Earth. This is applied to partial coverage and explicit longitude cropping
///     only.
/// </summary>
internal sealed class GetGlobalOffset(RenderOptions options, ILogger<GetGlobalOffset> logger) : StepBody, IActivityStepBody
{
    public Activity? Activity { get; set; }
    internal double GlobalOffset { get; private set; }

    public override ExecutionResult Run(IStepExecutionContext context)
    {
        GlobalOffset = GetOffset();
        logger.LogDebug("Global offset: {Offset:F2} degrees", Angle.FromRadians(GlobalOffset).Degrees);

        return ExecutionResult.Next();
    }

    private double GetOffset()
    {
        ArgumentNullException.ThrowIfNull(Activity);
        if (Activity.IsFullEarthCoverage()) return 0;

        // Explicit longitude crop
        var equirectangularRender = options.EquirectangularRender;
        if (equirectangularRender is { ExplicitCrop: true, LongitudeRange: not null })
        {
            return -equirectangularRender.LongitudeRange.Value.Start;
        }

        // Stitched crop, offsetting by the smallest longitude
        return
            -Activity.Registrations
                .Where(r => !r.LongitudeRange!.Value.OverlappingLeft)
                .Min(r => r.LongitudeRange!.Value.Range.Start)
                .NormaliseLongitude();
    }
}

internal static class GetGlobalOffsetExtensions
{
    internal static IStepBuilder<TData, GetGlobalOffset> GetGlobalOffset<TStep, TData>(this IStepBuilder<TData, TStep> builder)
        where TStep : IStepBody
        where TData : StitchWorkflowData
        => builder
            .Then<TStep, GetGlobalOffset, TData>("Calculate global offset")
            .Input(step => step.Activity, data => data.Activity)
            .Output(data => data.GlobalOffset, step => step.GlobalOffset);
}