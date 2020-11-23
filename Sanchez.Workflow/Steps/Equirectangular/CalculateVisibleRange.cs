using System.Linq;
using Ardalis.GuardClauses;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Projections;
using Sanchez.Processing.Services;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using Sanchez.Workflow.Models.Steps;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Equirectangular
{
    internal sealed class CalculateVisibleRange : StepBody, IActivityStepBody
    {
        private readonly IProjectionOverlapCalculator _projectionOverlapCalculator;
        private readonly RenderOptions _options;

        public CalculateVisibleRange(IProjectionOverlapCalculator projectionOverlapCalculator, RenderOptions options)
        {
            _projectionOverlapCalculator = projectionOverlapCalculator;
            _options = options;
        }

        public Activity? Activity { get; set; }

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            Guard.Against.Null(Activity, nameof(Activity));

            // Calculates overlapping regions between satellites, or visible region if not stitching images.
            _projectionOverlapCalculator.Initialise(Activity.Registrations.Select(p => p.Definition));

            // Set latitude and longitude ranges based on overlapping satellite ranges
            Activity.Registrations.ForEach(registration =>
            {
                registration.LatitudeRange = new ProjectionRange(registration.Definition.LatitudeRange);
                registration.LongitudeRange =
                    _options.StitchImages
                        ? _projectionOverlapCalculator.GetNonOverlappingRange(registration.Definition)
                        : new ProjectionRange(registration.Definition.LongitudeRange.UnwrapLongitude());
            });

            return ExecutionResult.Next();
        }
    }

    internal static class CalculateVisibleRangeExtensions
    {
        internal static IStepBuilder<TData, CalculateVisibleRange> CalculateVisibleRange<TData>(this IWorkflowBuilder<TData> builder)
            where TData : WorkflowData
            => builder
                .StartWith<CalculateVisibleRange, TData>("Calculate visible range")
                .WithActivity();

        internal static IStepBuilder<TData, CalculateVisibleRange> CalculateVisibleRange<TStep, TData>(this IStepBuilder<TData, TStep> builder)
            where TStep : IStepBody
            where TData : WorkflowData
            => builder
                .Then<TStep, CalculateVisibleRange, TData>("Calculate visible range")
                .WithActivity();
    }
}