using Funhouse.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace Funhouse.ImageProcessing.Mask
{
    /// <summary>
    ///     Sets the background outside the Earth on geostationary images to be transparent in order to facilitate blending.
    /// </summary>
    public readonly struct RemoveBackgroundRowOperation : IRowOperation
    {
        private readonly Image<Rgba32> _source;

        /// <summary>
        ///     Semi-major axis, relative to image size.
        /// </summary>
        private readonly double _semiMajor;

        /// <summary>
        ///     Semi-minor axis, relative to image size.
        /// </summary>
        private readonly double _semiMinor;

        /// <summary>
        ///     Semi-minor axis, squared.
        /// </summary>
        private readonly double _semiMinor2;

        /// <summary>
        ///     Semi-major axis, squared.
        /// </summary>
        private readonly double _semiMajor2;

        /// <param name="source">source image</param>
        public RemoveBackgroundRowOperation(Image<Rgba32> source)
        {
            _source = source;

            _semiMajor = source.Width / 2f;
            _semiMinor = source.Width / Constants.Earth.RadiusEquator * Constants.Earth.RadiusPolar / 2f;

            _semiMinor2 = _semiMinor * _semiMinor;
            _semiMajor2 = _semiMajor * _semiMajor;
        }

        public void Invoke(int y)
        {
            var span = _source.GetPixelRowSpan(y);

            for (var x = 0; x < span.Length; x++)
            {
                if (InEarth(x, y)) continue;
                span[x] = Constants.Transparent;
            }
        }

        /// <summary>
        ///     Returns whether a point is inside or outside the Earth
        /// </summary>
        private bool InEarth(int x, int y)
        {
            var xDistance = x - _semiMajor;
            var yDistance = y - _semiMinor;

            return xDistance * xDistance / _semiMajor2 + yDistance * yDistance / _semiMinor2 < 1;
        }
    }
}