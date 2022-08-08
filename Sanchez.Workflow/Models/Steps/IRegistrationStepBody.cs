using JetBrains.Annotations;
using Sanchez.Processing.Models.Projections;
using WorkflowCore.Interface;

namespace Sanchez.Workflow.Models.Steps;

/// <summary>
///     Workflow step which has a <see cref="Registration"/> input.
/// </summary>
public interface IRegistrationStepBody : IStepBody
{
    Registration? Registration { get; [UsedImplicitly] set; }
}