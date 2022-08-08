using JetBrains.Annotations;
using Sanchez.Processing.Models;

namespace Sanchez.Workflow.Models.Data;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public record EquirectangularWorkflowData : StitchWorkflowData
{
    /// <summary>
    ///     Batch activities for single item rendering.
    /// </summary>
    public List<Activity> Activities { get; init; } = null!;
}