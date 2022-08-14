namespace Sanchez.Processing.Models;

public class GeostationaryRenderOptions
{
    public GeostationaryRenderOptions(
        Angle? longitude, 
        Angle? endLongitude, 
        bool inverseRotation, 
        float atmosphereAmount,
        float atmosphereOpacity)
    {
        InverseRotation = inverseRotation;
        Longitude = longitude?.Radians;
        EndLongitude = endLongitude?.Radians;
        AtmosphereAmount = atmosphereAmount;
        AtmosphereOpacity = atmosphereOpacity;
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
    ///     Amount of atmosphere to apply to image; valid values are between 0 (no atmosphere) and 1 (full atmosphere).
    /// </summary>
    public float AtmosphereAmount { get; }
        
    /// <summary>
    ///     Opacity of atmosphere; valid values are between 0 (transparent) and 1 (opaque).
    /// </summary>
    public float AtmosphereOpacity { get; }
}