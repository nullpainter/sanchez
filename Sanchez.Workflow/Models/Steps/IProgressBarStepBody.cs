using JetBrains.Annotations;
using ShellProgressBar;
using WorkflowCore.Interface;

namespace Sanchez.Workflow.Models.Steps;

/// <summary>
///     Workflow step which has a <see cref="IProgressBar"/> input.
/// </summary>
public interface IProgressBarStepBody : IStepBody
{
    IProgressBar? ProgressBar { get; [UsedImplicitly] set; }
}