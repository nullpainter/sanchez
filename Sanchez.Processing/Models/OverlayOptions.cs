namespace Sanchez.Processing.Models;

public record OverlayOptions
{
    /// <summary>
    ///     Whether to apply a false-colour CLUT to IR imagery.
    /// </summary>
    public bool ApplyOverlay { get; set; }

    /// <summary>
    ///     Minimum scaled pixel intensity to apply CLUT. Expected values are from
    ///     0.0 - 1.0.
    /// </summary>
    public float MinIntensity { get; set; }

    /// <summary>
    ///     Maximum scaled pixel intensity to apply CLUT. Expected values are from
    ///     0.0 - 1.0.
    /// </summary>
    public float MaxIntensity { get; set; }

    /// <summary>
    ///     Path to CLUT gradient definition.
    /// </summary>
    public string GradientPath { get; set; } = Constants.DefaultGradientPath;
}