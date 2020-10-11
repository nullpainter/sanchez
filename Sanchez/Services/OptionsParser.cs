using System;
using Sanchez.Models.CommandLine;
using Sanchez.Processing.Extensions;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Configuration;
using Sanchez.Processing.Models.Options;

namespace Sanchez.Services
{
    public static class OptionsParser
    {
        public static RenderOptions Populate(GeostationaryOptions options)
        {
            var renderOptions = ProcessBaseOptions(options);
            
            renderOptions.GeostationaryRender = new GeostationaryRenderOptions(
                ToOptionalAngle(options.LongitudeDegrees),
                ToOptionalAngle(options.EndLongitudeDegrees),                
                options.InverseRotation,
                options.HazeAmount);

            return renderOptions;

            static Angle? ToOptionalAngle(double? longitudeDegrees) => longitudeDegrees == null ? (Angle?) null : Angle.FromDegrees(longitudeDegrees.Value);
        }

        public static RenderOptions Populate(EquirectangularOptions options)
        {
            var renderOptions = ProcessBaseOptions(options);

            renderOptions.EquirectangularRender = new EquirectangularRenderOptions(
                options.AutoCrop,
                options.Timestamp != null || options.IntervalMinutes != null, null
                /*ExtentsHelper.ParseExtentsString(options.Extents)*/);

            return renderOptions;
        }

        private static RenderOptions ProcessBaseOptions(CommandLineOptions options)
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
                SourcePath = options.SourcePath!,
                OutputPath = options.OutputPath!,
                OverlayPath = options.OverlayPath,
                InterpolationType = ToInterpolationType(options.InterpolationType),
                ImageSize = ToImageSize(options),
                ImageOffset = ToImageOffset(options),
                Force = options.Force,
                Timestamp = options.Timestamp?.DateTime,
                EndTimestamp = options.EndTimestamp?.DateTime,
                Interval = options.IntervalMinutes == null ? (TimeSpan?) null : TimeSpan.FromMinutes(options.IntervalMinutes.Value),
                Tolerance = TimeSpan.FromMinutes(options.ToleranceMinutes),
                AutoAdjustLevels = !options.NoAutoAdjustLevels,
                MinSatellites = options.MinSatellites
            };

            if (options.UnderlayPath != null) renderOptions.UnderlayPath = options.UnderlayPath;
            if (options.DefinitionsPath != null) renderOptions.DefinitionsPath = options.DefinitionsPath;

            return renderOptions;
        }

        private static ImageOffset ToImageOffset(CommandLineOptions options)
        {
            return options.SpatialResolution switch
            {
                Constants.Satellite.SpatialResolution.TwoKm => Constants.Satellite.Offset.TwoKm,
                Constants.Satellite.SpatialResolution.FourKm => Constants.Satellite.Offset.FourKm,
                _ => throw new ArgumentOutOfRangeException($"Unsupported spatial resolution: {options.SpatialResolution}")
            };
        }

        private static int ToImageSize(CommandLineOptions options)
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
    }
}