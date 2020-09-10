using System;
using Funhouse.Extensions;
using Funhouse.Models;
using Funhouse.Models.CommandLine;
using Funhouse.Models.Configuration;

namespace Funhouse
{
    public static class OptionsParser
    {
        private static RenderOptions ProcessBaseOptions(BaseOptions options)
        {
            var renderOptions = new RenderOptions
            {
                Quiet = options.Quiet,
                Verbose = options.Verbose,
                SpatialResolution = options.SpatialResolution,
                Brightness = options.Brightness,
                Saturation = options.Saturation,
                Tint = options.Tint.FromHexString()!.Value,
                NoUnderlay = options.NoUnderlay,
                OutputPath = options.OutputPath,
                SourcePath = options.SourcePath!,
                InterpolationType = ToInterpolationType(options.InterpolationType),
                ImageSize = ToImageSize(options),
                ImageOffset = ToImageOffset(options),
                Force = options.Force,
                Tolerance = TimeSpan.FromMinutes(options.ToleranceMinutes),
                TargetTimestamp = options.TargetTimestamp
            };

            if (options.UnderlayPath != null) renderOptions.UnderlayPath = options.UnderlayPath;

            return renderOptions;
        }


        private static ImageOffset ToImageOffset(BaseOptions options)
        {
            return options.SpatialResolution switch
            {
                Constants.Satellite.SpatialResolution.TwoKm => Constants.Satellite.Offset.TwoKm,
                Constants.Satellite.SpatialResolution.FourKm => Constants.Satellite.Offset.FourKm,
                _ => throw new ArgumentOutOfRangeException($"Unsupported spatial resolution: {options.SpatialResolution}")
            };
        }

        private static int ToImageSize(BaseOptions options)
        {
            return options.SpatialResolution switch
            {
                Constants.Satellite.SpatialResolution.TwoKm => Constants.Satellite.ImageSize.TwoKm,
                Constants.Satellite.SpatialResolution.FourKm => Constants.Satellite.ImageSize.FourKm,
                _ => throw new ArgumentOutOfRangeException($"Unsupported spatial resolution: {options.SpatialResolution}")
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

        public static RenderOptions Populate(GeostationaryOptions options)
        {
            var renderOptions = ProcessBaseOptions(options);
            renderOptions.GeostationaryRender = new GeostationaryRenderOptions(options.Longitude, options.HazeAmount);

            return renderOptions;
        }

        public static RenderOptions Populate(EquirectangularOptions options)
        {
            var renderOptions = ProcessBaseOptions(options);

            renderOptions.EquirectangularRender = new EquirectangularRenderOptions(options.AutoCrop);
            return renderOptions;
        }
    }
}