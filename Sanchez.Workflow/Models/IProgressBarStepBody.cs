using JetBrains.Annotations;
using ShellProgressBar;
using WorkflowCore.Interface;

namespace Sanchez.Workflow.Models
{
    public interface IProgressBarStepBody : IStepBody
    {
        IProgressBar? ProgressBar { get; [UsedImplicitly] set; }
    }
}