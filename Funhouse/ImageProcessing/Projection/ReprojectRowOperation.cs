using System;
using System.Collections.Generic;
using Ardalis.GuardClauses;
using Funhouse.Extensions;
using Funhouse.Models;
using Funhouse.Models.Angles;
using Funhouse.Models.Configuration;
using Funhouse.Models.Projections;
using Funhouse.Projections;
using MathNet.Spatial.Units;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using Range = Funhouse.Models.Angles.Range;

namespace Funhouse.ImageProcessing.Projection
{
    public readonly struct ReprojectRowOperation : IRowOperation
    {
        private readonly ProjectionActivity _activity;
        private readonly Image<Rgba32> _target;
        private readonly CommandLineOptions _options;
        private readonly IProjection _targetProjection;
        private readonly float _xOffset;
        private readonly Range _longitudeRange;

        private static readonly Rgba32 Transparent = new Rgba32(0, 0, 0, 0);
        
        /// <summary>
        ///     Longitude outside of the longitude range where the image is blended.
        /// </summary>
        private readonly Angle _blendEndLongitude;
        
        /// <summary>
        ///    Ratio of source image which has an alpha mask applied to blend it with overlapping images. 
        /// </summary>
        private const float BlendRatio = 0.05f;

        public ReprojectRowOperation(
            ProjectionActivity activity,
            Image<Rgba32> target,
            float xOffset,
            CommandLineOptions options,
            IProjection targetProjection)
        {
            _activity = activity;
            _target = target;
            _xOffset = xOffset;
            _options = options;
            _targetProjection = targetProjection;
            
            // Normalise longitude range so it doesn't wrap around the map
            _longitudeRange = activity.LongitudeRange.UnwrapLongitude();

            // Calculate end longitude for blend
            var overlap = Angle.FromRadians(BlendRatio * (_longitudeRange.End - _longitudeRange.Start).Radians);
            _blendEndLongitude = _longitudeRange.End + overlap;
            
            Guard.Against.Null(activity.Source, nameof(activity.Source));
            Guard.Against.Null(activity.Definition, nameof(activity.Definition));
        }

        private static readonly Dictionary<int, LatitudeCalculations> LatitudeCalculationCache = new Dictionary<int, LatitudeCalculations>();

        public void Invoke(int y)
        {
            var span = _target.GetPixelRowSpan(y);

            // Calculate or retrieve the latitude calculation component of geostationary projection
            var latitudeCalculations = CalculateGeostationaryLatitude(y);

            // Convert image x,y to Mercator projection angle
            var targetWidth = _activity.Source!.Width * 2;
            
            for (var x = 0; x < span.Length; x++)
            {
                var projectionAngleX = ProjectionAngle.FromX((int) Math.Round(x + _xOffset), targetWidth);
                var longitude = _targetProjection.ToLongitude(projectionAngleX);

                // Convert latitude/longitude to geostationary scanning angle
                var scanningAngle = GeostationaryProjection.FromLongitude(latitudeCalculations, longitude,_activity.Definition!);

                // Map pixel from satellite image back to target image
                span[x] = GetTargetColour(_activity, scanningAngle, longitude);
            }
        }

        /// <summary>
        ///     Performs latitude calculations as part of lat/long to scanning angle. These are cached in order to avoid
        ///     repeating latitude calculations for every pixel.
        /// </summary>
        private LatitudeCalculations CalculateGeostationaryLatitude(int y)
        {
            // Return cache value if present
            if (LatitudeCalculationCache.TryGetValue(y, out var latitudeCalculations)) return latitudeCalculations;

            // Convert pixel row to projection angle
            var projectionAngle = ProjectionAngle.FromY(_activity.Source!.Height - y, _target.Height);

            // Convert projection angle to latitude
            var latitude = _targetProjection.ToLatitude(projectionAngle);

            // Perform and cache intermediary geostationary latitude calculations
            latitudeCalculations = GeostationaryProjection.LatitudeCalculations(latitude);
            LatitudeCalculationCache[y] = latitudeCalculations;

            return latitudeCalculations;
        }

        /// <summary>
        ///     Gets the target pixel colour for a given scanning angle and longitude. Images are blended on their right-hand
        ///     side with an alpha gradient in order to smooth the transition between overlapping images.
        /// </summary>
        /// <param name="activity">source image and satellite definition</param>
        /// <param name="scanningAngle">geostationary scanning angle</param>
        /// <param name="longitude"></param>
        /// <returns>interpolated pixel value from source image at the given scanning angle</returns>
        private Rgba32 GetTargetColour(ProjectionActivity activity, ScanningAngle? scanningAngle, Angle longitude)
        {
            // Ignore pixels outside of disc and outside of crop region
            if (scanningAngle == null || longitude < _longitudeRange.Start || longitude > _blendEndLongitude) return Transparent;

            // Map pixel from source image if not blending
            if (longitude <= _longitudeRange.End) return InterpolatePixel(activity, scanningAngle.Value);

            // Blending, so identify target alpha
            var alpha = 1 - (longitude.Radians - _longitudeRange.End.Radians) / (_blendEndLongitude.Radians - _longitudeRange.End.Radians);
            
            var pixel = InterpolatePixel(activity, scanningAngle.Value);
            pixel.A = (byte) Math.Round(alpha * pixel.A);

            return pixel;
        }

        /// <summary>
        ///     Returns the interpolated pixel at a specified scanning angle.
        /// </summary>
        private Rgba32 InterpolatePixel(ProjectionActivity activity, ScanningAngle scanningAngle)
        {
            // Convert geostationary scanning angle to image coordinates
            var targetPosition = GeostationaryProjection.ToImageCoordinates(scanningAngle, activity.Definition!);

            // Interpolate fractional source pixel to target image
            var targetPixel = _options.InterpolationType switch
            {
                InterpolationType.NearestNeighbour => activity.Source!.NearestNeighbour(targetPosition),
                InterpolationType.Bilinear => activity.Source!.Bilinear(targetPosition),
                _ => throw new ArgumentOutOfRangeException($"Unhandled interpolation type: {_options.InterpolationType}")
            };

            return targetPixel;
        }
    }
}