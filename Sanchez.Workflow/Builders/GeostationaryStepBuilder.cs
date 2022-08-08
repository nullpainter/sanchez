using Microsoft.Extensions.DependencyInjection;
using Sanchez.Workflow.Steps.Geostationary;
using Sanchez.Workflow.Steps.Geostationary.Reprojected;

namespace Sanchez.Workflow.Builders;

internal static class GeostationaryStepBuilder
{
    /// <summary>
    ///     Registers workflow steps which are required for geostationary projection.
    /// </summary>
    internal static IServiceCollection AddGeostationarySteps(this IServiceCollection services)
    {
        return services
            .AddTransient<SetTargetLongitude>()
            .AddTransient<ToGeostationary>()
            .AddTransient<RenderUnderlay>()
            .AddTransient<ApplyHaze>();
    } 
}