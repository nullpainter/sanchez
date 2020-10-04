using Sanchez.Processing.Models.Projections;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Common
{
    internal sealed class SetWorkflowRegistration : StepBody
    {
        public override ExecutionResult Run(IStepExecutionContext context)
        {
            var registration = (Registration) context.Item;
            var data = (WorkflowData) context.Workflow.Data;

            data.Registration = registration;
            return ExecutionResult.Next();
        }
    }

    internal static class SetWorkflowRegistrationExtensions
    {
        internal static IStepBuilder<TData, SetWorkflowRegistration> SetWorkflowRegistration<TData>(this IWorkflowBuilder<TData> builder)
            where TData : WorkflowData
            => builder
                .StartWith<SetWorkflowRegistration, TData>();
    }
}