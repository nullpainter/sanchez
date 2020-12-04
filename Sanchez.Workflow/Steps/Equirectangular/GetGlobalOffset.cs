using System;
using System.Linq;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Sanchez.Processing.Extensions;
using Sanchez.Processing.Models;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using Sanchez.Workflow.Models.Steps;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Equirectangular
{
    /// <summary>
    ///     Calculates the horizontal offset to be applied to stitched IR images so the images don't
    ///     wrap around the Earth. This is applied to partial coverage and explicit longitude cropping
    ///     only.
    /// </summary>
    internal sealed class GetGlobalOffset : StepBody, IActivityStepBody
    {
        private readonly RenderOptions _options;
        private readonly ILogger<GetGlobalOffset> _logger;
        public Activity? Activity { get; set; }
        internal double GlobalOffset { get; private set; }

        public GetGlobalOffset(RenderOptions options, ILogger<GetGlobalOffset> logger)
        {
            _options = options;
            _logger = logger;
        }

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            GlobalOffset = GetOffset();
            _logger.LogDebug("Global offset: {offset:F2} degrees", Angle.FromRadians(GlobalOffset).Degrees);

            return ExecutionResult.Next();
        }

        private double GetOffset()
        {
            Guard.Against.Null(Activity, nameof(Activity));
            var equirectangularRender = _options.EquirectangularRender;

            if (equirectangularRender != null)
            {
                // Offset by explicit or default start longitude for full earth coverage
                if (Activity.IsFullEarthCoverage())
                {
                    return -equirectangularRender.StartLongitude.Radians;
                }

                // Offset by explicit longitude crop
                if (equirectangularRender.ExplicitCrop && equirectangularRender.LongitudeRange != null)
                {
                    return -equirectangularRender.LongitudeRange.Value.Start;
                }
            }

            // Stitched crop, offsetting by the smallest longitude
            return
                -Activity.Registrations
                    .Where(r => !r.LongitudeRange!.OverlappingLeft)
                    .Min(r => r.LongitudeRange!.Range.Start)
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
}