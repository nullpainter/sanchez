using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace Funhouse.ImageProcessing.Blur
{
    /// <summary>
    ///     Applies a blur to the edge of the Earth disc in order to avoid artifacts in the projected image.
    /// </summary>
    public readonly struct BlurEdgesRowOperation : IRowOperation
    {
        private readonly float _amount;
        private readonly Image<Rgba32> _source;

        public BlurEdgesRowOperation(float amount, Image<Rgba32> source)
        {
            _amount = amount;
            _source = source;
        }

        public void Invoke(int y)
        {
            var span = _source.GetPixelRowSpan(y);
            
            for (var x = 0; x < span.Length; x++)
            {
                var distance = InsideEarth(x, y, span.Length);
                if (distance <= _amount) continue;
                
                var alpha = distance >= 1 ? 0 : (1 - distance) / (distance - _amount);
                
                var pixel = span[x];
                span[x] = new Rgba32(pixel.R / 255f, pixel.G / 255f, pixel.B / 255f, (float) alpha);
            }
        }
        
        /// <summary>
        ///     Returns whether a point is inside or outside the earth
        /// </summary>
        /// <returns>1 if point is on circumference, > 1 if outside earth</returns>
        private static double InsideEarth(int x, int y, int spatialResolution)
        {
            var semiMajor = spatialResolution / 2f;
            var semiMinor = spatialResolution / Constants.Earth.RadiusEquator * Constants.Earth.RadiusPolar / 2f;

            var xDistance = x - semiMajor;
            var yDistance = y - semiMajor;

            return xDistance * xDistance / (semiMajor * semiMajor) + yDistance * yDistance / (semiMinor * semiMinor);
        }
    }
}