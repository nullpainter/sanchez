using System;
using System.Collections.Concurrent;
using Ardalis.GuardClauses;
using Funhouse.Extensions.Images;
using Funhouse.Models;
using Funhouse.Models.Angles;
using Funhouse.Models.Configuration;
using Funhouse.Models.Projections;
using Funhouse.Projections;
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
        private readonly int _xOffset, _yOffset;
        private readonly Range _latitudeRange, _longitudeRange;

        /// <summary>
        ///     Longitude outside of the longitude range where the image is blended.
        /// </summary>
        private readonly double _blendEndLongitude;

        /// <summary>
        ///     Ratio of source image which has an alpha mask applied to blend it with overlapping images.
        /// </summary>
        private const float BlendRatio = 0.05f;

        public ReprojectRowOperation(ProjectionActivity activity,
            Image<Rgba32> target,
            int xOffset,
            int yOffset,
            RenderOptions options) 
        {
            Guard.Against.Null(activity.Definition, nameof(activity.Definition));
            Guard.Against.Null(activity.Source, nameof(activity.Source));

            _activity = activity;
            _target = target;
            _xOffset = xOffset;
            _yOffset = yOffset;
            _options = options;

            _sourceBuffer = ImageBuffer.ToBuffer(activity.Source);
            _imageOffset = options.ImageOffset;

            // Normalise longitude range so it doesn't wrap around the map
            _longitudeRange = activity.LongitudeRange.UnwrapLongitude();
            _latitudeRange = activity.Definition.LatitudeRange;

            // Calculate end longitude for blend
            var overlap = BlendRatio * (_longitudeRange.End - _longitudeRange.Start);
            _blendEndLongitude = _longitudeRange.End + overlap;

            Guard.Against.Null(activity.Source, nameof(activity.Source));
            Guard.Against.Null(activity.Definition, nameof(activity.Definition));
        }

        private static readonly ConcurrentDictionary<int, LatitudeCalculations> LatitudeCalculationCache = new ConcurrentDictionary<int, LatitudeCalculations>();
        private readonly ImageBuffer _sourceBuffer;
        private readonly ImageOffset _imageOffset;
        private readonly RenderOptions _options;

        public void Invoke(int y)
        {
            var span = _target.GetPixelRowSpan(y);

            // Calculate or retrieve the latitude calculation component of geostationary projection
            var latitudeCalculations = CalculateGeostationaryLatitude(y + _yOffset);

            // Convert image x,y to Mercator projection angle
            var targetWidth = _activity.Source!.Width * 2;
            var projectionY = ProjectionAngle.FromY(y + _yOffset, _activity.Source!.Height);

            for (var x = 0; x < span.Length; x++)
            {
                var projectionX = ProjectionAngle.FromX(x + _xOffset, targetWidth);

                // Convert latitude/longitude to geostationary scanning angle
                GeostationaryProjection.ToScanningAngle(latitudeCalculations, projectionX, _activity.Definition!, out var scanningX, out var scanningY);

                // Map pixel from satellite image back to target image
                span[x] = GetTargetColour(scanningX, scanningY, projectionY, projectionX);
            }
        }

        /// <summary>
        ///     Performs latitude calculations as part of lat/long to scanning angle. These are cached in order to avoid
        ///     repeating latitude calculations for every pixel.
        /// </summary>
        private LatitudeCalculations CalculateGeostationaryLatitude(int y)
        {
            var target = _target;
            var yOffset = _yOffset;

            return LatitudeCalculationCache.GetOrAdd(y, angle =>
            {
                // Convert pixel row to latitude
                var projectionY = ProjectionAngle.FromY(y, target.Height + yOffset * 2);

                // Perform and cache intermediary geostationary latitude calculations
                return GeostationaryProjection.LatitudeCalculations(-projectionY);
            });
        }

        /// <summary>
        ///     Gets the target pixel colour for a given scanning angle and longitude. Images are blended on their right-hand
        ///     side with an alpha gradient in order to smooth the transition between overlapping images.
        /// </summary>
        /// <param name="scanningX">geostationary scanning angle</param>
        /// <param name="scanningY">geostationary scanning angle</param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns>interpolated pixel value from source image at the given scanning angle</returns>
        private Rgba32 GetTargetColour(double scanningX, double scanningY, double latitude, double longitude)
        {
            // Ignore pixels outside of disc and outside of crop region
            if (double.IsNaN(scanningX) || double.IsNaN(scanningY)
                                        || longitude < _longitudeRange.Start
                                        || longitude > _blendEndLongitude
                                        || latitude < _latitudeRange.Start
                                        || latitude > _latitudeRange.End) return Constants.Transparent;

            // Map pixel from source image if not blending
            if (longitude <= _longitudeRange.End) return InterpolatePixel(scanningX, scanningY);

            // Blending, so identify target alpha
            var alpha = 1 - (longitude - _longitudeRange.End) / (_blendEndLongitude - _longitudeRange.End);

            // Calculate target pixel and blend
            var pixel = InterpolatePixel(scanningX, scanningY);

            pixel.A = (byte) Math.Round(alpha * pixel.A);
            return pixel;
        }

        /// <summary>
        ///     Returns the interpolated pixel at a specified scanning angle.
        /// </summary>
        private Rgba32 InterpolatePixel(double scanningX, double scanningY)
        {
            // Convert geostationary scanning angle to image coordinates
            _imageOffset.ToImageCoordinates(scanningX, scanningY, out var targetX, out var targetY);
            return _sourceBuffer.GetInterpolatedPixel(targetX, targetY, _options.InterpolationType);
        }
    }
}