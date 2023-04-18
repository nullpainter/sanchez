using JetBrains.Annotations;
using SixLabors.ImageSharp;

namespace Sanchez.Workflow.Models.Data;

/// <summary>
///     Data backing workflows which stitch multiple satellite images together.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public record StitchWorkflowData : WorkflowData
{
    /// <summary>
    ///     Longitude offset in radians to apply to all images so the first satellite is at
    ///     horizontal position 0px.
    /// </summary>
    public double GlobalOffset { get; init; }

    public Rectangle? CropBounds { get; init; }
}