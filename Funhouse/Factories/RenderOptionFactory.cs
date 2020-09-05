using System;
using Funhouse.Extensions;
using Funhouse.Models;
using Funhouse.Models.CommandLine;
using Funhouse.Models.Configuration;
using static Funhouse.Models.Constants.Satellite;
using ProjectionOptions = Funhouse.Models.CommandLine.ProjectionOptions;

namespace Funhouse.Factories
{
    internal static class RenderOptionFactory
    {
        /// <summary>
        ///     Creates render options from command line options.
        /// </summary>
        internal static RenderOptions ToRenderOptions(CommandLineOptions options)
        {
            return new RenderOptions(
                options.Brightness,
                options.Saturation,
                options.Tint!.FromHexString()!.Value,
                options.HazeAmount,
                ToInterpolationType(options.InterpolationType),
                ToProjectionType(options.ProjectionType),
                ToImageOffset(options),
                ToImageSize(options));
        }

        private static ProjectionType ToProjectionType(ProjectionOptions projectionType)
        {
            return projectionType switch
            {
                ProjectionOptions.E => ProjectionType.Equirectangular,
                ProjectionOptions.G => ProjectionType.Geostationary,
                _ => throw new ArgumentOutOfRangeException($"Unsupported projection type: {projectionType}")
            };
        }

        private static InterpolationType ToInterpolationType(InterpolationOptions interpolationType)
        {
            return interpolationType switch
            {
                InterpolationOptions.B => InterpolationType.Bilinear,
                InterpolationOptions.N => InterpolationType.NearestNeighbour,
                _ => throw new ArgumentOutOfRangeException($"Unsupported interpolation type: {interpolationType}")
            };
        }

        private static int ToImageSize(CommandLineOptions options)
        {
            return options.SpatialResolution switch
            {
                SpatialResolution.TwoKm => ImageSize.TwoKm,
                SpatialResolution.FourKm => ImageSize.FourKm,
                _ => throw new ArgumentOutOfRangeException($"Unsupported spatial resolution: {options.SpatialResolution}")
            };
        }

        private static ImageOffset ToImageOffset(CommandLineOptions options)
        {
            return options.SpatialResolution switch
            {
                SpatialResolution.TwoKm => Offset.TwoKm,
                SpatialResolution.FourKm => Offset.FourKm,
                _ => throw new ArgumentOutOfRangeException($"Unsupported spatial resolution: {options.SpatialResolution}")
            };
        }
    }
}