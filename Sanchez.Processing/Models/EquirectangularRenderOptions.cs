using Sanchez.Processing.Models.Angles;

namespace Sanchez.Processing.Models;

public record EquirectangularRenderOptions(
    bool AutoCrop, 
    bool NoCrop,
    bool StitchImages, 
    AngleRange? LatitudeRange = null,
    AngleRange? LongitudeRange = null)
{
    /// <summary>
    ///     Whether multiple source files are to be stitched together or rendered individually.
    /// </summary>
    public bool StitchImages { get; } = StitchImages;

    /// <summary>
    ///     Optional latitude crop.
    /// </summary>
    public AngleRange? LatitudeRange { get; } = LatitudeRange;

    /// <summary>
    ///     Optional longitude crop.
    /// </summary>
    public AngleRange? LongitudeRange { get; } = LongitudeRange;

    /// <summary>
    ///     Whether the user has specified crop bounds.
    /// </summary>
    public bool ExplicitCrop => LatitudeRange != null || LongitudeRange != null;
}