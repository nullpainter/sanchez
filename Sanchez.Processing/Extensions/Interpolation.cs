namespace Sanchez.Processing.Extensions;

public static class Interpolation
{
    /// <summary>
    ///     Linear interpolation between two values.
    /// </summary>
    /// <param name="start">first value</param>
    /// <param name="end">second value</param>
    /// <param name="amount">amount between 0.0 - 1.0</param>
    /// <returns>interpolated value</returns>
    public static double Lerp(double start, double end, double amount) => start + (end - start) * amount;

    /// <summary>
    ///     Bilinear interpolation.
    /// </summary>
    public static double Blerp(double c00, double c10, double c01, double c11, double positionX, double positionY)
    {
        return Lerp(Lerp(c00, c10, positionX), Lerp(c01, c11, positionX), positionY);
    }
}