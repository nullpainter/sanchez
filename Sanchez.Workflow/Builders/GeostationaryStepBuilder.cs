using Microsoft.Extensions.DependencyInjection;
using Sanchez.Workflow.Steps.Geostationary;
using Sanchez.Workflow.Steps.Geostationary.Reprojected;

namespace Sanchez.Workflow.Builders
{
    internal static class GeostationaryStepBuilder
    {
        internal static IServiceCollection AddGeostationarySteps(this IServiceCollection services)
        {
            return services
                .AddTransient<ToGeostationary>()
                .AddTransient<RenderUnderlay>()
                .AddTransient<ApplyHaze>();
        } 
    }
}