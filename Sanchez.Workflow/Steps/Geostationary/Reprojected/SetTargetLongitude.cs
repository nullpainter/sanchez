using System;
using System.Collections.Generic;
using Ardalis.GuardClauses;
using Sanchez.Processing.Extensions;
using Sanchez.Processing.Models;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using Range = Sanchez.Processing.Models.Angles.Range;

namespace Sanchez.Workflow.Steps.Geostationary.Reprojected
{
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
            Guard.Against.Null(TargetTimestamp, nameof(TargetTimestamp));

            var geostationaryOptions = _options.GeostationaryRender!;

            Longitude = geostationaryOptions.EndLongitude == null
                ? geostationaryOptions.Longitude
                : GetTimelapseLongitude(geostationaryOptions);

            return ExecutionResult.Next();
        }

        private double GetTimelapseLongitude(GeostationaryRenderOptions geostationaryOptions)
        {
            Guard.Against.Null(TimeIntervals, nameof(TimeIntervals));
            Guard.Against.Zero(TimeIntervals.Count, nameof(TimeIntervals));

            var inverse = geostationaryOptions.InverseRotation;
            
            var rawIndex = TimeIntervals.IndexOf(TargetTimestamp!.Value);
            
            // Note that the default start and end longitude are inverted by default to simplify maths
            var currentIndex = inverse ? rawIndex : TimeIntervals.Count - 1 - rawIndex;
            if (currentIndex < 0) throw new InvalidOperationException($"Unable to find timestamp {TargetTimestamp} in timelapse");

            var end = inverse ? geostationaryOptions.EndLongitude!.Value : geostationaryOptions.Longitude!.Value;
            var start = inverse ? geostationaryOptions.Longitude!.Value : geostationaryOptions.EndLongitude!.Value;

            var range = new Range(start, end).UnwrapLongitude();
            var offset = (range.End - range.Start) * (currentIndex / ((double) TimeIntervals.Count - 1));

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
}