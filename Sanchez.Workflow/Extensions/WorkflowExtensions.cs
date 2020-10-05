using Sanchez.Workflow.Models.Data;
using Sanchez.Workflow.Models.Steps;
using WorkflowCore.Interface;

namespace Sanchez.Workflow.Extensions
{
    internal static class WorkflowExtensions
    {
        /// <summary>
        ///     Convenience wrapper around <see cref="IWorkflowModifier{TData,TStepBody}.Then{TStep}(System.Action{WorkflowCore.Interface.IStepBuilder{TData,TStep}})"/>
        ///     to create named workflow steps.
        /// </summary>
        /// <param name="builder">workflow builder</param>
        /// <param name="name">optional step name; if not provided, step name is derived from <see cref="TStep"/>'s class</param>
        internal static IStepBuilder<TData, TStep> Then<TSourceStep, TStep, TData>(this IStepBuilder<TData, TSourceStep> builder, string? name = null)
            where TSourceStep : IStepBody
            where TStep : IStepBody
            where TData : IWorkflowData
            => builder.Then<TStep>(b => b.Name(name ?? typeof(TStep).Name));

        /// <summary>
        ///     Convenience wrapper around <see cref="IWorkflowBuilder{TData}.StartWith{TStep}"/> to create named workflow steps.
        /// </summary>
        /// <param name="builder">workflow builder</param>
        /// <param name="name">optional step name; if not provided, step name is derived from <see cref="TStep"/>'s class</param>
        internal static IStepBuilder<TData, TStep> StartWith<TStep, TData>(this IWorkflowBuilder<TData> builder, string? name = null)
            where TStep : IStepBody
            where TData : IWorkflowData
            => builder.StartWith<TStep>(b => b.Name(name ?? typeof(TStep).Name));

        /// <summary>
        ///     Adds image registration input property mapping to an <see cref="IRegistrationStepBody"/>.
        /// </summary>
        internal static IStepBuilder<TData, TStep> WithRegistration<TStep, TData>(this IStepBuilder<TData, TStep> builder)
            where TStep : IRegistrationStepBody
            where TData : WorkflowData
            => builder.Input(step => step.Registration, data => data.Registration);

        /// <summary>
        ///     Adds activity input property mapping to an <see cref="IActivityStepBody"/>.
        /// </summary>
        internal static IStepBuilder<TData, TStep> WithActivity<TStep, TData>(this IStepBuilder<TData, TStep> builder)
            where TStep : IActivityStepBody
            where TData : WorkflowData
            => builder.Input(step => step.Activity, data => data.Activity);

        /// <summary>
        ///     Adds progress bar property mapping to a <see cref="IProgressBarStepBody"/>.
        /// </summary>
        internal static IStepBuilder<TData, TStep> WithProgressBar<TStep, TData>(this IStepBuilder<TData, TStep> builder)
            where TStep : IProgressBarStepBody
            where TData : WorkflowData
            => builder.Input(step => step.ProgressBar, data => data.ProgressBar);
    }
}