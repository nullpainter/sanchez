using Sanchez.Processing.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace Sanchez.Processing.ImageProcessing.Mask
{
    /// <summary>
    ///     Sets the background outside the Earth on geostationary images to be transparent in order to facilitate blending.
    /// </summary>
    public readonly struct RemoveBackgroundRowOperation : IRowOperation
    {
        private readonly Image<Rgba32> _source;

        /// <summary>
        ///     Semi-minor axis, squared.
        /// </summary>
        private readonly double _semiMinor2;

        /// <summary>
        ///     Semi-major axis, squared.
        /// </summary>
        private readonly double _semiMajor2;

        /// <summary>
        ///     Adjustment factor for whether a point is inside the Earth, necessary to avoid single-pixel rounding errors
        ///     with Himawari-8's white background.
        /// </summary>
        private const double BorderRatio = 0.001d;

        /// <param name="source">source image</param>
        public RemoveBackgroundRowOperation(Image<Rgba32> source)
        {
            _source = source;

            var semiMajor = source.Width / 2d;
            var semiMinor = source.Height * (Constants.Earth.RadiusPolar / Constants.Earth.RadiusEquator) / 2d;

            _semiMinor2 = semiMinor * semiMinor;
            _semiMajor2 = semiMajor * semiMajor;
        }

        public void Invoke(int y)
        {
            var span = _source.GetPixelRowSpan(y);

            for (var x = 0; x < span.Length; x++)
            {
                if (InEarth(x, y)) continue;
                
                // Set zero alpha for areas outside the earth
                span[x] = Constants.Transparent;
            }
        }

        /// <summary>
        ///     Returns whether a point is inside or outside the Earth
        /// </summary>
        private bool InEarth(int x, int y)
        {
            var xDistance = x - _source.Width / 2d;
            var yDistance = y - _source.Height / 2d;

            return xDistance * xDistance / _semiMajor2 + yDistance * yDistance / _semiMinor2 < 1 - BorderRatio;
        }
    }
}