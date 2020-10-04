using Microsoft.Extensions.DependencyInjection;
using Sanchez.Workflow.Services;

namespace Sanchez.Workflow.Builders
{
    public static class WorkflowBuilder
    {
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