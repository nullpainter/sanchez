using MathNet.Spatial.Units;

namespace Funhouse.Models.Configuration
{
    public class ImageOffset
    {
        public ImageOffset(Angle x, Angle y, double scaleFactor)
        {
            X = x;
            Y = y;
            ScaleFactor = scaleFactor;
        }

        public Angle X { get; set; }
        public Angle Y { get; set; }
        public double ScaleFactor { get; set; }
    }
}