using Sanchez.Workflow.Models;
using Sanchez.Workflow.Steps.Common;
using WorkflowCore.Interface;

namespace Sanchez.Workflow.Extensions
{
    public static class StepExtensions
    {
        internal static IStepBuilder<TData, GetSourceFiles> Initialise<TData>(this IWorkflowBuilder<TData> builder)
            where TData : WorkflowData
            => builder
                .InitialiseUnderlayCache()
                .InitialiseSatelliteRegistry()
                .GetSourceFiles();
    }
}