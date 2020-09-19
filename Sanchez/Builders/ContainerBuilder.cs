using Sanchez.Compositors;
using Sanchez.Helpers;
using Sanchez.Models;
using Sanchez.Seeder;
using Sanchez.Services;
using Sanchez.Services.Equirectangular;
using Sanchez.Services.Filesystem;
using Sanchez.Services.Underlay;
using SimpleInjector;

namespace Sanchez.Builders
{
    public static class ContainerBuilder
    {
        public static Container AddAllService(this Container container, RenderOptions options)
        {
            container.RegisterCompositors();
            container.RegisterUnderlaySupport();
            
            container.RegisterSingleton<IImageStitcher, ImageStitcher>();
            container.RegisterSingleton<ISatelliteImageLoader, SatelliteImageLoader>();
            container.RegisterSingleton<IDatabaseMigrator, DatabaseMigrator>();
            container.RegisterSingleton<IEquirectangularImageRenderer, EquirectangularImageRenderer>();
            container.RegisterSingleton<IImageMatcher, ImageMatcher>();
            container.RegisterSingleton<IFileService, FileService>();
            container.RegisterSingleton<IProjectionOverlapCalculator, ProjectionOverlapCalculator>();
            container.RegisterSingleton<ISatelliteRegistry, SatelliteRegistry>();
            container.RegisterSingleton<Sanchez>();
            container.RegisterSingleton<FilenameParserProvider>();

            container.RegisterInstance(ProgressBarFactory.NewProgressBar(options));
            container.RegisterInstance(options);

            return container;
        }

        private static void RegisterUnderlaySupport(this Container container)
        {
            container.RegisterSingleton<IUnderlayCache, UnderlayCache>();
            container.RegisterSingleton<IUnderlayService, UnderlayService>();
            container.RegisterSingleton<IUnderlayCacheRepository, UnderlayCacheRepository>();
        }

        private static void RegisterCompositors(this Container container)
        {
            container.RegisterSingleton<IGeostationaryCompositor, GeostationaryCompositor>();
            container.RegisterSingleton<IEquirectangularCompositor, EquirectangularCompositor>();
            container.RegisterSingleton<ICompositor, Compositor>();
        }
    }
}