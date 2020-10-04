using Sanchez.Workflow.Models;
using WorkflowCore.Interface;

namespace Sanchez.Workflow.Extensions
{
    internal static class WorkflowExtensions
    {
        internal static IStepBuilder<TData, TStep> Then<TSourceStep, TStep, TData>(this IStepBuilder<TData, TSourceStep> builder, string? name = null)
            where TSourceStep : IStepBody
            where TStep : IStepBody
            where TData : IWorkflowData
            => builder.Then<TStep>(b => b.Name(name ?? typeof(TStep).Name));

        internal static IStepBuilder<TData, TStep> StartWith<TStep, TData>(this IWorkflowBuilder<TData> builder, string? name = null)
            where TStep : IStepBody
            where TData : IWorkflowData
            => builder.StartWith<TStep>(b => b.Name(name ?? typeof(TStep).Name));
        
        internal static IStepBuilder<TData, TStep> WithRegistration<TStep, TData>(this IStepBuilder<TData, TStep> builder)
            where TStep : IRegistrationStepBody
            where TData : WorkflowData
            => builder.Input(step => step.Registration, data => data.Registration);
        
        internal static IStepBuilder<TData, TStep> WithActivity<TStep, TData>(this IStepBuilder<TData, TStep> builder)
            where TStep : IActivityStepBody
            where TData: WorkflowData
            => builder.Input(step => step.Activity, data => data.Activity);
        
           
        internal static IStepBuilder<TData, TStep> WithProgressBar<TStep, TData>(this IStepBuilder<TData, TStep> builder)
            where TStep : IProgressBarStepBody
            where TData: WorkflowData
            => builder.Input(step => step.ProgressBar, data => data.ProgressBar);
    }
}