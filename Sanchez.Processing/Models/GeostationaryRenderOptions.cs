namespace Sanchez.Processing.Models;

public class GeostationaryRenderOptions
{
    public GeostationaryRenderOptions(
        Angle? longitude, 
        Angle? endLongitude, 
        bool inverseRotation, 
        float hazeAmount,
        float hazeOpacity)
    {
        InverseRotation = inverseRotation;
        Longitude = longitude?.Radians;
        EndLongitude = endLongitude?.Radians;
        HazeAmount = hazeAmount;
        HazeOpacity = hazeOpacity;
    }

    /// <summary>
    ///     Whether Earth rotation should be performed in a counter-clockwise manner when rotating from <see cref="Longitude"/>
    ///     to <see cref="EndLongitude"/>.
    /// </summary>
    public bool InverseRotation { get; }

    /// <summary>
    ///     Target longitude in radians for geostationary satellite projection.
    /// </summary>
    public double? Longitude { get; }

    /// <summary>
    ///     End longitude in radians for timelapse geostationary satellite projection.
    /// </summary>
    public double? EndLongitude { get; }

    /// <summary>
    ///     Amount of haze to apply to image; valid values are between 0 (no haze) and 1 (full haze).
    /// </summary>
    public float HazeAmount { get; }
        
    /// <summary>
    ///     Opacity of haze; valid values are between 0 (transparent) and 1 (opaque).
    /// </summary>
    public float HazeOpacity { get; }
}