using Funhouse.Models;
using Funhouse.Services;
using SimpleInjector;

namespace Funhouse.Builders
{
    public static class ContainerBuilder
    {
        public static Container AddAllService(this Container container, CommandLineOptions options)
        {
            container.RegisterSingleton<ICompositor, Compositor>();
            container.RegisterSingleton<IImageStitcher, ImageStitcher>();
            container.RegisterSingleton<IImageProjector, ImageProjector>();
            container.RegisterSingleton<IImageLoader, ImageLoader>();
            
            container.RegisterInstance(options);
            container.RegisterSingleton<IProjectionActivityOperations, ProjectionActivityOperations>();
            container.RegisterSingleton<IProjectionOverlapCalculator, ProjectionOverlapCalculator>();
            container.RegisterSingleton<ISatelliteRegistry, SatelliteRegistry>();
            container.RegisterSingleton<IProjectionRegistry, ProjectionRegistry>();

            return container;
        }
    }
}