using NUnit.Framework;
using Sanchez.Builders;
using SimpleInjector;

namespace Sanchez.Test
{
    public abstract class ServiceTests
    {
        protected Container Container { get; private set;}

        [OneTimeSetUp]
        public void CreateContainer() => Container = new Container().AddAllService();

        protected T GetService<T>() where T : class => Container.GetInstance<T>();
    }
}