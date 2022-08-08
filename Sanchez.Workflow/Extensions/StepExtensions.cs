using Sanchez.Workflow.Models.Data;
using Sanchez.Workflow.Steps.Common;
using WorkflowCore.Interface;

namespace Sanchez.Workflow.Extensions;

public static class StepExtensions
{
    /// <summary>
    ///     Perform initialisation steps common across all workflows.
    /// </summary>
    internal static IStepBuilder<TData, GetSourceFiles> Initialise<TData>(this IWorkflowBuilder<TData> builder)
        where TData : WorkflowData
        => builder
            .InitialiseUnderlayCache()
            .InitialiseSatelliteRegistry()
            .InitialiseGradient()
            .GetSourceFiles();
}