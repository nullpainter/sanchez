using Funhouse.Compositors;
using Funhouse.Helpers;
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
            container.RegisterCompositors();
            container.RegisterSingleton<IImageStitcher, ImageStitcher>();
            container.RegisterSingleton<ISatelliteImageLoader, SatelliteImageLoader>();
            container.RegisterSingleton<IUnderlayCache, UnderlayCache>();
            container.RegisterSingleton<IUnderlayService, UnderlayService>();
            container.RegisterSingleton<IUnderlayCacheRepository, UnderlayCacheRepository>();
            container.RegisterSingleton<IDatabaseMigrator, DatabaseMigrator>();
            container.RegisterSingleton<IEquirectangularImageRenderer, EquirectangularImageRenderer>();
            container.RegisterSingleton<IImageMatcher, ImageMatcher>();
            container.RegisterSingleton<IFileService, FileService>();
            container.RegisterSingleton<IConsoleLogger, ConsoleLogger>();
            container.RegisterSingleton<FilenameParserProvider>();

            container.RegisterInstance(options);

            container.RegisterSingleton<IProjectionOverlapCalculator, ProjectionOverlapCalculator>();
            container.RegisterSingleton<ISatelliteRegistry, SatelliteRegistry>();
            container.RegisterSingleton<Funhouse>();

            var progressBar = ProgressBarFactory.NewProgressBar(options);
            container.RegisterInstance(progressBar);

            return container;
        }

        public static Container RegisterCompositors(this Container container)
        {
            container.RegisterSingleton<IGeostationaryCompositor, GeostationaryCompositor>();
            container.RegisterSingleton<IEquirectangularCompositor, EquirectangularCompositor>();
            container.RegisterSingleton<ICompositor, Compositor>();

            return container;
        }
    }
}