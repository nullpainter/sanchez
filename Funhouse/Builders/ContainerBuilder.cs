using Funhouse.Models;
using Funhouse.Services;
using SimpleInjector;

namespace Funhouse.Builders
{
    public static class ContainerBuilder
    {
        public static Container AddAllService(this Container container, CommandLineOptions options, RenderOptions renderOptions)
        {
            container.RegisterSingleton<ICompositor, Compositor>();
            container.RegisterSingleton<IImageStitcher, ImageStitcher>();
            container.RegisterSingleton<IImageProjector, ImageProjector>();
            container.RegisterSingleton<IImageLoader, ImageLoader>();
            
            container.RegisterInstance(options);
            container.RegisterInstance(renderOptions);
            
            container.RegisterSingleton<IProjectionActivityOperations, ProjectionActivityOperations>();
            container.RegisterSingleton<IProjectionOverlapCalculator, ProjectionOverlapCalculator>();
            container.RegisterSingleton<ISatelliteRegistry, SatelliteRegistry>();
            container.RegisterSingleton<Funhouse>();

            return container;
        }
    }
}