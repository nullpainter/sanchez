using JetBrains.Annotations;
using Sanchez.Processing.Models;
using WorkflowCore.Interface;

namespace Sanchez.Workflow.Models
{
    public interface IActivityStepBody : IStepBody
    {
        Activity? Activity { get; [UsedImplicitly] set; }
    }
}