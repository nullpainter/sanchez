namespace Funhouse.Models.Configuration
{
    public class ImageOffset
    {
        // TODO note angles in radians
        public ImageOffset(double x, double y, double scaleFactor)
        {
            X = x;
            Y = y;
            ScaleFactor = scaleFactor;
        }

        public double X { get; }
        public double Y { get; }
        public double ScaleFactor { get; }
        
        /// <summary>
        ///     Returns the scanning angle of a geostationary image, given pixel coordinates.
        /// </summary>
        public double ToHorizontalScanningAngle(double x) => x * ScaleFactor + X;

        /// <summary>
        ///     Returns the scanning angle of a geostationary image given pixel coordinates.
        /// </summary>
        public double ToVerticalScanningAngle(double y) => y * ScaleFactor - Y;
        
        /// <summary>
        ///     Returns the pixel coordinates of a geostationary image given a scanning angle.
        /// </summary>
        public void ToImageCoordinates(double scanningX, double scanningY, out double x, out double y)
        {
            x = (scanningX - X) / ScaleFactor;
            y = (scanningY + Y) / ScaleFactor;
        }
    }
}