using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Angles;

namespace Sanchez.Processing.Extensions;

public static class PixelRangeExtensions
{
    /// <summary>
    ///     Converts an angle range to a X pixel range, assuming the target image has X pixel coordinates mapping from -180 to 180 degrees.
    /// </summary>
    public static PixelRange ToPixelRangeX(this AngleRange range, int width) => new(range, angle => angle.ToX(width));

    /// <summary>
    ///     Converts an angle range to a Y pixel range, assuming the target image has Y pixel coordinates mapping from 90 to -90 degrees.
    /// </summary>
    public static PixelRange ToPixelRangeY(this AngleRange range, int height) => new(range, angle => angle.ToY(height));

    public static int ToY(this double angle, int height) => (int) Math.Round(height - (angle + Constants.PiOver2) * height / Math.PI);
    public static int ToX(this double angle, int width) => (int) Math.Round((angle + Math.PI) * width / Constants.Pi2);
}