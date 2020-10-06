using JetBrains.Annotations;
using SixLabors.ImageSharp;

namespace Sanchez.Workflow.Models.Data
{
    /// <summary>
    ///     Data backing workflows which stitch multiple satellite images together.
    /// </summary>
    public class StitchWorkflowData : WorkflowData
    {
        /// <summary>
        ///     Longitude offset in radians to apply to all images so the first satellite is at
        ///     horizontal position 0px.
        /// </summary>
        public double GlobalOffset { get; [UsedImplicitly] set; }

        public Rectangle? CropBounds { get; [UsedImplicitly] set; } = null!;
    }
}