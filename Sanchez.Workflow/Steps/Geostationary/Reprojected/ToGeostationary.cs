using System;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Sanchez.Processing.ImageProcessing.ShadeEdges;
using Sanchez.Processing.ImageProcessing.Underlay;
using Sanchez.Processing.Models;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models;
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

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            Guard.Against.Null(_options.GeostationaryRender!.Longitude, nameof(GeostationaryRenderOptions.Longitude));
            Guard.Against.Null(Activity, nameof(Activity));
            Guard.Against.Null(TargetImage, nameof(TargetImage));

            var longitudeRange = Activity.GetVisibleLongitudeRange();

            // Determine visible range of all satellite imagery
            var longitudeDegrees = _options.GeostationaryRender!.Longitude!.Value;

            _logger.LogInformation("Reprojecting to geostationary with longitude {longitudeDegrees} degrees", longitudeDegrees);

            // Adjust longitude based on the underlay wrapping for visible satellites
            var adjustedLongitude = -Math.PI - longitudeRange.Start + Angle.FromDegrees(longitudeDegrees).Radians;

            // Render geostationary image
            using (var sourceImage = TargetImage.Clone())
            {
                TargetImage = sourceImage.ToGeostationaryProjection(adjustedLongitude, Constants.Satellite.DefaultHeight, _options);

                // Apply haze if required
                var hazeAmount = _options.GeostationaryRender.HazeAmount;
                if (hazeAmount > 0 && !_options.NoUnderlay)
                {
                    TargetImage.ApplyHaze(_options.Tint, hazeAmount);
                }
            }

            return ExecutionResult.Next();
        }

        public Activity? Activity { get; set; }
        public Image<Rgba32>? TargetImage { get; set; }
    }
    
    public static class ToGeostationaryExtensions
    {
        internal static IStepBuilder<EquirectangularStitchWorkflowData, ToGeostationary> ToGeostationary<TStep>(this IStepBuilder<EquirectangularStitchWorkflowData, TStep> builder)
            where TStep : IStepBody
            => builder
                .Then<TStep, ToGeostationary, EquirectangularStitchWorkflowData>("Reprojecting to geostationary")
                .WithActivity()
                .Input(step => step.TargetImage, data => data.TargetImage)
                .Output(data => data.TargetImage, step => step.TargetImage);
    }
}