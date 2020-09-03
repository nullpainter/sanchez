using Funhouse.Models;
using Funhouse.Models.CommandLine;
using Funhouse.Seeder;
using Funhouse.Services;
using Funhouse.Services.Underlay;
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
            container.RegisterSingleton<IUnderlayCache, UnderlayCache>();
            container.RegisterSingleton<IUnderlayService, UnderlayService>();
            container.RegisterSingleton<IUnderlayCacheRepository, UnderlayCacheRepository>();
            container.RegisterSingleton<IDatabaseMigrator, DatabaseMigrator>();
            
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