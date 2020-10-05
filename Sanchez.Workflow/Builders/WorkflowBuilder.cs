using Microsoft.Extensions.DependencyInjection;
using Sanchez.Workflow.Services;

namespace Sanchez.Workflow.Builders
{
    public static class WorkflowBuilder
    {
        /// <summary>
        ///     Adds application services supporting workflows, including all required workflow steps.
        /// </summary>
        public static IServiceCollection AddWorkflow(this IServiceCollection services)
        {
            return services
                .AddTransient<IWorkflowService, WorkflowService>()
                .AddCommonSteps()
                .AddGeostationarySteps()
                .AddEquirectangularSteps();
        }
    }
}