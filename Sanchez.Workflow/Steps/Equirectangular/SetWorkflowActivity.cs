using Sanchez.Processing.Models;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Equirectangular
{
    internal sealed class SetWorkflowActivity : StepBody
    {
        public override ExecutionResult Run(IStepExecutionContext context)
        {
            var activity = (Activity) context.Item;
            var data = (EquirectangularWorkflowData)context.Workflow.Data;

            data.Activity = activity;
            return ExecutionResult.Next();
        }
    }
    
    internal static class SetWorkflowActivityExtensions
    {
        internal static IStepBuilder<EquirectangularWorkflowData, SetWorkflowActivity> SetWorkflowActivity(this IWorkflowBuilder<EquirectangularWorkflowData> builder)
            => builder
                .StartWith<SetWorkflowActivity, EquirectangularWorkflowData>();
    }
}