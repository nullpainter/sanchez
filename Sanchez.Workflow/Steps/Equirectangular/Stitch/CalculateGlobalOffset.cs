using System.Linq;
using Ardalis.GuardClauses;
using Sanchez.Processing.Models;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Equirectangular.Stitch
{
    internal sealed class CalculateGlobalOffset : StepBody, IActivityStepBody
    {
        public Activity? Activity { get; set; }
        internal double GlobalOffset { get; private set; }

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            Guard.Against.Null(Activity, nameof(Activity));

            GlobalOffset = -Activity.Registrations
                .Select(p => p.LongitudeRange.UnwrapLongitude().NormaliseLongitude().Start)
                .Min();

            return ExecutionResult.Next();
        }
    }

    internal static class CalculateGlobalOffsetExtensions
    {
        internal static IStepBuilder<TData, CalculateGlobalOffset> CalculateGlobalOffset<TStep, TData>(this IStepBuilder<TData, TStep> builder)
            where TStep : IStepBody
            where TData : EquirectangularStitchWorkflowData
            => builder
                .Then<TStep, CalculateGlobalOffset, TData>("Calculate global offset")
                .Input(step => step.Activity, data => data.Activity)
                .Output(data => data.GlobalOffset, step => step.GlobalOffset);
    }
}