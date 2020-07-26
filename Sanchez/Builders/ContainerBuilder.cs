using Sanchez.Services;
using SimpleInjector;

namespace Sanchez.Builders
{
    public static class ContainerBuilder
    {
        public static Container AddAllService(this Container container)
        {
            container.Register<IFileService, FileService>();
            container.Register<ICompositor, Compositor>();
            container.Register<IOptionValidator, OptionValidator>();

            return container;
        }
    }
}