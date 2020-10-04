using JetBrains.Annotations;
using SixLabors.ImageSharp;

namespace Sanchez.Workflow.Models
{
    public class EquirectangularStitchWorkflowData : WorkflowData
    {
        public double GlobalOffset { get; [UsedImplicitly] set; }

        public Rectangle? CropBounds { get; [UsedImplicitly] set; } = null!;
    }
}