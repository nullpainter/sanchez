using NUnit.Framework;
using Sanchez.Builders;
using SimpleInjector;

namespace Sanchez.Test
{
    public abstract class ServiceTests
    {
        private Container Container { get; set;}

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Container = new Container().AddAllService();
        }

        protected T GetService<T>() where T : class => Container.GetInstance<T>();
    }
}