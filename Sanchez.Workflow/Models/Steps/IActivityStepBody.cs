using JetBrains.Annotations;
using Sanchez.Processing.Models;
using WorkflowCore.Interface;

namespace Sanchez.Workflow.Models.Steps
{
    /// <summary>
    ///     Workflow step which has an <see cref="Activity"/> input.
    /// </summary>
    public interface IActivityStepBody : IStepBody
    {
        Activity? Activity { get; [UsedImplicitly] set; }
    }
}