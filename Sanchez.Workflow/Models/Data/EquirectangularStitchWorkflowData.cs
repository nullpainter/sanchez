using JetBrains.Annotations;
using SixLabors.ImageSharp;

namespace Sanchez.Workflow.Models.Data
{
    /// <summary>
    ///     Data backing <see cref="Sanchez.Workflow.Workflows.Equirectangular.EquirectangularStitchWorkflow"/>.
    /// </summary>
    public class EquirectangularStitchWorkflowData : WorkflowData
    {
        /// <summary>
        ///     Longitude offset in radians to apply to all images so the first satellite is at
        ///     horizontal position 0px.
        /// </summary>
        public double GlobalOffset { get; [UsedImplicitly] set; }

        public Rectangle? CropBounds { get; [UsedImplicitly] set; } = null!;
    }
}