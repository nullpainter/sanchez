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
    // TODO document this plz
    // also, is this really needed as a separate step? only used by one other
    // note that it sets globalOffset in radians
    internal sealed class CalculateGlobalOffset : StepBody, IActivityStepBody
    {
        private readonly RenderOptions _options;
        private readonly ILogger<CalculateGlobalOffset> _logger;
        public Activity? Activity { get; set; }
        internal double GlobalOffset { get; private set; }

        public CalculateGlobalOffset(RenderOptions options, ILogger<CalculateGlobalOffset> logger)
        {
            _options = options;
            _logger = logger;
        }

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            GlobalOffset = GetGlobalOffset();
            _logger.LogDebug("Global offset: {offset:F2} degrees", Angle.FromRadians(GlobalOffset).Degrees);

            return ExecutionResult.Next();
        }

        private double GetGlobalOffset()
        {
            Guard.Against.Null(Activity, nameof(Activity));
            if (Activity.IsFullEarthCoverage()) return -Math.PI;

            var equirectangularRender = _options.EquirectangularRender!;
            if (equirectangularRender.ExplicitCrop && equirectangularRender.LongitudeRange != null)
            {
                return -equirectangularRender.LongitudeRange.Value.Start.NormaliseLongitude();
            }
            
 
            return 
                -Activity.Registrations
                    .Where(r => !r.LongitudeRange!.OverlappingLeft)
                    .Min(r => r.LongitudeRange!.Range.Start)
                    .NormaliseLongitude();
        }
    }

    internal static class CalculateGlobalOffsetExtensions
    {
        internal static IStepBuilder<TData, CalculateGlobalOffset> CalculateGlobalOffset<TStep, TData>(this IStepBuilder<TData, TStep> builder)
            where TStep : IStepBody
            where TData : StitchWorkflowData
            => builder
                .Then<TStep, CalculateGlobalOffset, TData>("Calculate global offset")
                .Input(step => step.Activity, data => data.Activity)
                .Output(data => data.GlobalOffset, step => step.GlobalOffset);
    }
}