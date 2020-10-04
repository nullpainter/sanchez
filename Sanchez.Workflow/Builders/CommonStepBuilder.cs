using Microsoft.Extensions.DependencyInjection;
using Sanchez.Workflow.Steps.Common;

namespace Sanchez.Workflow.Builders
{
    internal static class CommonStepBuilder
    {
        internal static IServiceCollection AddCommonSteps(this IServiceCollection services)
        {
            return services
                .AddTransient<GetSourceRegistrations>()
                .AddTransient<InitialiseProgressBar>()
                .AddTransient<InitialiseSatelliteRegistry>()
                .AddTransient<CreateActivity>()
                .AddTransient<LogCompletion>()
                .AddTransient<SaveImage>()
                .AddTransient<ShouldWriteSingle>()
                .AddTransient<SetWorkflowRegistration>()
                .AddTransient<InitialiseUnderlayCache>()
                .AddTransient<LoadImageSingle>()
                .AddTransient<ColourCorrect>()
                .AddTransient<NormaliseImage>();
        } 
    }
}