using System.Linq.Expressions;
using Sanchez.Processing.Models.Projections;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using Sanchez.Workflow.Models.Steps;
using ShellProgressBar;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Equirectangular;

internal sealed class LoadImage : StepBodyAsync, IRegistrationStepBody, IProgressBarStepBody
{
    public IProgressBar? ProgressBar { get; set; } = null!;
    public Registration? Registration { get; set; }

    public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(Registration);
        ArgumentNullException.ThrowIfNull(ProgressBar);

        ProgressBar.Message = $"Rendering {Path.GetFileName(Registration.Path)}";

        // Load image
        await Registration.LoadAsync();
            
        ProgressBar.Tick();
        return ExecutionResult.Next();
    }
}

internal static class LoadImageExtensions
{
    internal static IStepBuilder<TData, LoadImage> LoadImage<TStep, TData>(this IStepBuilder<TData, TStep> builder, Expression<Func<TData, IProgressBar?>> progressBar)
        where TStep : IStepBody
        where TData : WorkflowData
    {
        return builder
            .Then<TStep, LoadImage, TData>("Load image")
            .Input(step => step.ProgressBar, progressBar)
            .WithRegistration();
    }
}