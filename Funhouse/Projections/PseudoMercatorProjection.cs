using static System.Math;

namespace Funhouse.Projections
{
    public static class PseudoMercatorProjection
    {
        // y in degrees
        public static double YToLatitude(double y) => Atan(Exp(y / 180 * PI)) / PI * 360 - 90;

        // 
        public static double LatitudeToY(double latitude) => Log(Tan((latitude + 90) / 360 * PI)) / PI * 180;
    }
}