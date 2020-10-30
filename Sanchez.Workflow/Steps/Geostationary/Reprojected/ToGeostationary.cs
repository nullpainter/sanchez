using System;
using System.Linq.Expressions;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Sanchez.Processing.ImageProcessing.ShadeEdges;
using Sanchez.Processing.ImageProcessing.Underlay;
using Sanchez.Processing.Models;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using Sanchez.Workflow.Models.Steps;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Geostationary.Reprojected
{
    /// <summary>
    ///     Renders an equirectangular image into geostationary projection.
    /// </summary>
    /// <remarks>
    ///    This step is used when repositioning a geostationary image. To do this, the satellite
    ///    image is first projected to equirectangular projection and composited with the underlay.
    /// </remarks>
    public class ToGeostationary : StepBody, IActivityStepBody
    {
        private readonly ILogger<ToGeostationary> _logger;
        private readonly RenderOptions _options;

        public ToGeostationary(
            ILogger<ToGeostationary> logger,
            RenderOptions options)
        {
            _logger = logger;
            _options = options;
        }

        /// <summary>
        ///     Target longitude, in radians.
        /// </summary>
        internal double? Longitude { get; set; }

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            Guard.Against.Null(Longitude, nameof(Longitude));
            Guard.Against.Null(Activity, nameof(Activity));
            Guard.Against.Null(TargetImage, nameof(TargetImage));

            var longitudeRange = Activity.GetVisibleLongitudeRange();

            // Determine visible range of all satellite imagery
            _logger.LogInformation("Reprojecting to geostationary with longitude {longitude} degrees", Angle.FromRadians(Longitude!.Value).Degrees);

            // Adjust longitude based on the underlay wrapping for visible satellites
            var adjustedLongitude = -Math.PI - longitudeRange.Start + Longitude!.Value;

            // Render geostationary image
            using (var sourceImage = TargetImage.Clone())
            {
                TargetImage = sourceImage.ToGeostationaryProjection(adjustedLongitude, Constants.Satellite.DefaultHeight, _options);
            }

            return ExecutionResult.Next();
        }

        public Activity? Activity { get; set; }
        public Image<Rgba32>? TargetImage { get; set; }
    }

    public static class ToGeostationaryExtensions
    {
        internal static IStepBuilder<TData, ToGeostationary> ToGeostationary<TStep, TData>(
            this IStepBuilder<TData, TStep> builder, 
            Expression<Func<TData, double?>> longitude)
            where TStep : IStepBody
            where TData : WorkflowData
            => builder
                .Then<TStep, ToGeostationary, TData>("Reprojecting to geostationary")
                .WithActivity()
                .Input(step => step.Longitude, longitude)
                .Input(step => step.TargetImage, data => data.TargetImage)
                .Output(data => data.TargetImage, step => step.TargetImage);

        internal static IStepBuilder<TData, ToGeostationary> ToGeostationary<TData>(
            this IWorkflowBuilder<TData> builder,
            Expression<Func<TData, double?>> longitude,
            Expression<Func<TData, Image<Rgba32>?>> image)
            where TData : WorkflowData
            => builder
                .StartWith<ToGeostationary, TData>("Reprojecting to geostationary")
                .WithActivity()
                .Input(step => step.Longitude, longitude)
                .Input(step => step.TargetImage, image)
                .Output(image, step => step.TargetImage);
    }
}