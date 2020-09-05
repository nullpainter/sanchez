namespace Funhouse.Models.Configuration
{
    public class ImageOffset
    {
        /// <summary>
        ///     Creates a new image offset, providing values to convert pixel coordinates to geostationary scanning angles.
        /// </summary>
        /// <param name="xOffset">horizontal offset in radians</param>
        /// <param name="yOffset">vertical offset in radians</param>
        /// <param name="scaleFactor">horizontal and vertical scale factor</param>
        public ImageOffset(double xOffset, double yOffset, double scaleFactor)
        {
            XOffset = xOffset;
            YOffset = yOffset;
            ScaleFactor = scaleFactor;
        }

        private double XOffset { get; }
        private double YOffset { get; }
        private double ScaleFactor { get; }

        /// <summary>
        ///     Returns the scanning angle of a geostationary image, given pixel coordinates.
        /// </summary>
        /// <returns>horizontal scanning angle in radians</returns>
        public double ToHorizontalScanningAngle(int x) => x * ScaleFactor + XOffset;

        /// <summary>
        ///     Returns the scanning angle of a geostationary image, given pixel coordinates.
        /// </summary>
        /// <returns>vertical scanning angle in radians</returns>
        public double ToVerticalScanningAngle(int y) => y * -ScaleFactor + YOffset;

        /// <summary>
        ///     Returns the pixel coordinates of a geostationary image given a scanning angle.
        /// </summary>
        /// <param name="scanningX">Horizontal scanning angle in radians</param>
        /// <param name="scanningY">Vertical scanning angle in radians</param>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>

        public void ToImageCoordinates(double scanningX, double scanningY, out double x, out double y)
        {
            x = (scanningX - XOffset) / ScaleFactor;
            y = (scanningY - YOffset) / -ScaleFactor;
        }
    }
}