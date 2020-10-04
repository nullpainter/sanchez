using Microsoft.Extensions.DependencyInjection;
using Sanchez.Workflow.Steps.Equirectangular;
using Sanchez.Workflow.Steps.Equirectangular.Stitch;
using Sanchez.Workflow.Steps.Equirectangular.Timelapse;
using ShouldWrite = Sanchez.Workflow.Steps.Equirectangular.Stitch.ShouldWrite;

namespace Sanchez.Workflow.Builders
{
    public static class EquirectangularStepBuilder
    {
        internal static IServiceCollection AddEquirectangularSteps(this IServiceCollection services)
        {
            return services
                .AddSteps()
                .AddBatchSteps()
                .AddTimelapseSteps();
        }

        private static IServiceCollection AddTimelapseSteps(this IServiceCollection services)
        {
            return services
                .AddTransient<InitialiseImageProgressBar>()
                .AddTransient<SetTargetTimestamp>()
                .AddTransient<CreateActivities>()
                .AddTransient<PrepareTimeIntervals>();
        }

        private static IServiceCollection AddBatchSteps(this IServiceCollection services)
        {
            return services
                .AddTransient<CropImage>()
                .AddTransient<LoadImage>()
                .AddTransient<ShouldWrite>()
                .AddTransient<StitchImages>()
                .AddTransient<CropImage>()
                .AddTransient<RenderUnderlay>()
                .AddTransient<SaveStitchedImage>()
                .AddTransient<GetCropBounds>()
                .AddTransient<CalculateVisibleRange>()
                .AddTransient<CalculateGlobalOffset>()
                .AddTransient<ToEquirectangular>();
        }

        private static IServiceCollection AddSteps(this IServiceCollection services)
        {
            return services
                .AddTransient<RegisterImages>()
                .AddTransient<SetWorkflowActivity>();
        }
    }
}