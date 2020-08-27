﻿using Funhouse.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace Funhouse.ImageProcessing.Mask
{
    /// <summary>
    ///     Applies a blur to the edge of the Earth disc in order to avoid artifacts in the projected image.
    /// </summary>
    public readonly struct BlurEdgeRowOperation : IRowOperation
    {
        private readonly Image<Rgba32> _source;
        private readonly float _blurAmount;

        public BlurEdgeRowOperation(Image<Rgba32> source, float? blurAmount = null)
        {
            _source = source;
            _blurAmount = blurAmount ?? 0.995f;
        }

        public void Invoke(int y)
        {
            var span = _source.GetPixelRowSpan(y);

            for (var x = 0; x < span.Length; x++)
            {
                // Determine if the point is inside the bounds of the Earth
                var distance = GetDistance(x, y, span.Length);

                // Don't mask pixels within the blur amount. The blur is applied inside the Earth's radius and a
                // distance of 1 indicates a point on the circumference.
                if (distance < _blurAmount) continue;

                // Apply a mask by adjusting the alpha channel. If we are outside the Earth's radius, the alpha
                // is always set to 0. Otherwise, we calculate an alpha gradient based on the distance from the 
                // blur tolerance to the radius.
                var alpha = distance >= 1 ? 0 : (1 - distance) / (distance - _blurAmount);
                if (alpha > 1) alpha = 1;

                span[x].A = (byte)(alpha * 255);
            }
        }

        /// <summary>
        ///     Returns whether a point is inside or outside the earth
        /// </summary>
        /// <returns>1 if point is on circumference, > 1 if outside earth</returns>
        private static double GetDistance(int x, int y, int spatialResolution)
        {
            var semiMajor = spatialResolution / 2f;
            var semiMinor = spatialResolution / Constants.Earth.RadiusEquator * Constants.Earth.RadiusPolar / 2f;

            var xDistance = x - semiMajor;
            var yDistance = y - semiMajor;

            return xDistance * xDistance / (semiMajor * semiMajor) + yDistance * yDistance / (semiMinor * semiMinor);
        }
    }
}