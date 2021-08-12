using System;
using Sanchez.Models.CommandLine;
using Sanchez.Processing.Extensions;
using Sanchez.Processing.Helpers;
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
                options.HazeAmount,
                options.HazeOpacity);

            return renderOptions;

            static Angle? ToOptionalAngle(double? longitudeDegrees) => longitudeDegrees == null ? (Angle?) null : Angle.FromDegrees(longitudeDegrees.Value);
        }

        public static RenderOptions Populate(EquirectangularOptions options)
        {
            var renderOptions = ProcessBaseOptions(options);

            renderOptions.EquirectangularRender = new EquirectangularRenderOptions(
                options.AutoCrop,
                options.NoCrop,
                options.Timestamp != null || options.IntervalMinutes != null,
                RangeHelper.ParseRange(options.LatitudeRange),
                RangeHelper.ParseRange(options.LongitudeRange));

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
                InterpolationType = ToInterpolationType(options.InterpolationType),
                ImageSize = ToImageSize(options),
                ImageOffset = ToImageOffset(options),
                Force = options.Force,
                Timestamp = options.Timestamp?.DateTime,
                EndTimestamp = options.EndTimestamp?.DateTime,
                Interval = options.IntervalMinutes == null ? (TimeSpan?) null : TimeSpan.FromMinutes(options.IntervalMinutes.Value),
                Tolerance = TimeSpan.FromMinutes(options.ToleranceMinutes),
                AutoAdjustLevels = !options.NoAutoAdjustLevels,
                AdaptiveLevelAdjustment = options.AdaptiveLevelAdjustment,
                MinSatellites = options.MinSatellites,
                OutputFormat = ToOutputFormat(options)
            };

            if (options.UnderlayPath != null) renderOptions.UnderlayPath = options.UnderlayPath;
            if (options.DefinitionsPath != null) renderOptions.DefinitionsPath = options.DefinitionsPath;

            SetOverlayOptions(options, renderOptions);

            return renderOptions;
        }

       private static ImageFormats? ToOutputFormat(CommandLineOptions options)
       {
           if (options.OutputFormat == null) return null;

           return options.OutputFormat.ToLower() switch
           {
               Constants.SupportedExtensions.Png => ImageFormats.Png,
               Constants.SupportedExtensions.Jpg => ImageFormats.Jpeg,
               _ => throw new ArgumentOutOfRangeException($"Unsupported output format: {options.OutputFormat}")
           };
       }

       private static void SetOverlayOptions(CommandLineOptions options, RenderOptions renderOptions)
        {
            renderOptions.Overlay.ApplyOverlay = options.ClutRange != null;
            if (!renderOptions.Overlay.ApplyOverlay || options.ClutRange == null) return;

            if (options.GradientPath != null) renderOptions.Overlay.GradientPath = options.GradientPath;

            var intensityRange = options.ClutRange.Split('-');
            renderOptions.Overlay.MinIntensity = float.Parse(intensityRange[0]);
            renderOptions.Overlay.MaxIntensity = float.Parse(intensityRange[1]);
        }

        private static ImageOffset ToImageOffset(CommandLineOptions options)
        {
            return options.SpatialResolution switch
            {
                Constants.Satellite.SpatialResolution.HalfKm => Constants.Satellite.Offset.HalfKm,
                Constants.Satellite.SpatialResolution.OneKm => Constants.Satellite.Offset.OneKm,
                Constants.Satellite.SpatialResolution.TwoKm => Constants.Satellite.Offset.TwoKm,
                Constants.Satellite.SpatialResolution.FourKm => Constants.Satellite.Offset.FourKm,
                _ => throw new ArgumentOutOfRangeException($"Unsupported spatial resolution: {options.SpatialResolution}")
            };
        }

        private static int ToImageSize(CommandLineOptions options)
        {
            return options.SpatialResolution switch
            {
                Constants.Satellite.SpatialResolution.HalfKm => Constants.Satellite.ImageSize.HalfKm,
                Constants.Satellite.SpatialResolution.OneKm => Constants.Satellite.ImageSize.OneKm,
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