using System.Collections.Generic;
using JetBrains.Annotations;
using Sanchez.Processing.Models;

namespace Sanchez.Workflow.Models
{
    public class EquirectangularWorkflowData : EquirectangularStitchWorkflowData
    {
        /// <summary>
        ///     Batch activities for single item rendering.
        /// </summary>
        public List<Activity> Activities { get; [UsedImplicitly] set; } = null!;
    }
}