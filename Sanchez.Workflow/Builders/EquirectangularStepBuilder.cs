using Microsoft.Extensions.DependencyInjection;
using Sanchez.Workflow.Steps.Equirectangular;
using Sanchez.Workflow.Steps.Equirectangular.Stitch;
using Sanchez.Workflow.Steps.Equirectangular.Timelapse;

namespace Sanchez.Workflow.Builders
{
    public static class EquirectangularStepBuilder
    {
        /// <summary>
        ///     Registers workflow steps which are required for equirectangular projection.
        /// </summary>
        internal static IServiceCollection AddEquirectangularSteps(this IServiceCollection services)
        {
            return AddCommonSteps(services)
                .AddBatchSteps()
                .AddTimelapseSteps();
        }

        /// <summary>
        ///     Registers workflow steps which are required for <see cref="Sanchez.Workflow.Workflows.Equirectangular.EquirectangularTimelapseWorkflow" />.
        /// </summary>
        private static IServiceCollection AddTimelapseSteps(this IServiceCollection services)
        {
            return services
                .AddTransient<InitialiseImageProgressBar>()
                .AddTransient<SetTargetTimestamp>()
                .AddTransient<CreateActivities>()
                .AddTransient<PrepareTimeIntervals>();
        }

        /// <summary>
        ///     Registers workflow steps which are required for batch equirectangular processing.
        /// </summary>
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

        /// <summary>
        ///     Registers common equirectangular projection steps.
        /// </summary>
        private static IServiceCollection AddCommonSteps(this IServiceCollection services)
        {
            return services
                .AddTransient<RegisterImages>()
                .AddTransient<SetWorkflowActivity>();
        }
    }
}
