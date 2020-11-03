using System;
using Sanchez.Processing.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace Sanchez.Processing.ImageProcessing.ShadeEdges
{
    public readonly struct HazeRowOperation : IRowOperation
    {
        private readonly Image<Rgba32> _source;
        private readonly Rgba32 _tint;

        /// <summary>
        ///     Semi-minor axis, squared.
        /// </summary>
        private readonly double _semiMinor2;

        /// <summary>
        ///     Semi-major axis, squared.
        /// </summary>
        private readonly double _semiMajor2;

        private readonly float _hazeAmount;
        
        /// <summary>
        ///     Adjustment factor for whether a point is inside the Earth, necessary to avoid single-pixel rounding errors
        ///     with Himawari-8's white background.
        /// </summary>
        private const double BorderRatio = 0.001d;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="tint"></param>
        /// <param name="hazeAmount">Amount of haze to apply, from 0.0 - 1.0</param>
        public HazeRowOperation(Image<Rgba32> source, Rgba32 tint, float hazeAmount)
        {
            _source = source;
            _tint = tint;

            var semiMajor = source.Width / 2d;
            var semiMinor = source.Height * (Constants.Earth.RadiusPolar / Constants.Earth.RadiusEquator) / 2d;

            _semiMinor2 = semiMinor * semiMinor;
            _semiMajor2 = semiMajor * semiMajor;

            // Adjust haze alpha based on tint
            _hazeAmount = hazeAmount * (_tint.A / 255f);
        }

        public void Invoke(int y)
        {
            var span = _source.GetPixelRowSpan(y);

            for (var x = 0; x < span.Length; x++)
            {
                var distance = Distance(x, y);
                if (distance < 1 - _hazeAmount|| distance > 1) continue;

                var alpha = (distance - 1) / _hazeAmount;

                span[x] = _tint;
                span[x].A = (byte) Math.Round(alpha * 255);
            }
        }

        /// <summary>
        ///     Distance from point to Earth's circumference. A value of 1 is on the circumference, and less
        ///     than 1 is inside the Earth.
        /// </summary>
        private double Distance(int x, int y)
        {
            var xDistance = x - _source.Width / 2d;
            var yDistance = y - _source.Height / 2d;

            return xDistance * xDistance / _semiMajor2 + yDistance * yDistance / _semiMinor2 - BorderRatio;
        }
    }
}