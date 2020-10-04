using Microsoft.Extensions.DependencyInjection;
using Sanchez.Processing.Builders;
using Sanchez.Processing.Models;
using Sanchez.Workflow.Builders;

namespace Sanchez.Builders
{
    public static class ContainerBuilder
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, RenderOptions options)
        {
            return services
                .AddProcessing()
                .AddWorkflow()
                .AddSingleton(options);
        }
    }
}