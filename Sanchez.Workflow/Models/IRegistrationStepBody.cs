using JetBrains.Annotations;
using Sanchez.Processing.Models.Projections;
using WorkflowCore.Interface;

namespace Sanchez.Workflow.Models
{
    public interface IRegistrationStepBody : IStepBody
    {
        Registration? Registration { get; [UsedImplicitly] set; }
    }
}