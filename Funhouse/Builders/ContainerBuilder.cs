using Funhouse.Models;
using Funhouse.Seeder;
using Funhouse.Services;
using Funhouse.Services.Equirectangular;
using Funhouse.Services.Filesystem;
using Funhouse.Services.Underlay;
using SimpleInjector;

namespace Funhouse.Builders
{
    public static class ContainerBuilder
    {
        public static Container AddAllService(this Container container, RenderOptions options)
        {
            container.RegisterSingleton<ICompositor, Compositor>();
            container.RegisterSingleton<IImageStitcher, ImageStitcher>();
            container.RegisterSingleton<IImageLoader, ImageLoader>();
            container.RegisterSingleton<IUnderlayCache, UnderlayCache>();
            container.RegisterSingleton<IUnderlayService, UnderlayService>();
            container.RegisterSingleton<IUnderlayCacheRepository, UnderlayCacheRepository>();
            container.RegisterSingleton<IDatabaseMigrator, DatabaseMigrator>();
            container.RegisterSingleton<IEquirectangularImageRenderer, EquirectangularImageRenderer>();
            container.RegisterSingleton<IImageLocator, ImageLocator>();
            container.RegisterSingleton<FilenameParserProvider>();
            
            container.RegisterInstance(options);
            
            container.RegisterSingleton<IProjectionActivityOperations, ProjectionActivityOperations>();
            container.RegisterSingleton<IProjectionOverlapCalculator, ProjectionOverlapCalculator>();
            container.RegisterSingleton<ISatelliteRegistry, SatelliteRegistry>();
            container.RegisterSingleton<Funhouse>();

            return container;
        }
    }
}