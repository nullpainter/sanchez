using System.Linq;
using Ardalis.GuardClauses;
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
    internal sealed class CalculateGlobalOffset : StepBody, IActivityStepBody
    {
        public Activity? Activity { get; set; }
        internal double GlobalOffset { get; private set; }

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            Guard.Against.Null(Activity, nameof(Activity));
            
            GlobalOffset = Activity.IsFullEarthCoverage() ? 0d : 
             -Activity.Registrations
                .Where(r => !r.LongitudeRange.OverlappingLeft)
                .Min(r => r.LongitudeRange.Range.Start)
                .NormaliseLongitude();

            return ExecutionResult.Next();
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