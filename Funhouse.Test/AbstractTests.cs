using System.IO;
using System.Threading.Tasks;
using Funhouse.Builders;
using Funhouse.Models;
using Funhouse.Services;
using NUnit.Framework;
using SimpleInjector;
using SixLabors.ImageSharp;

namespace Funhouse.Test
{
    public abstract class AbstractTests
    {
        /// <summary>
        ///     Small number for floating-point comparison tests.
        /// </summary>
        protected const float Precision = 0.000001f;

        private Container Container { get; set; }

        private static string DefinitionsPath => Path.Combine(TestContext.CurrentContext.TestDirectory, Constants.DefinitionsPath);

        protected ISatelliteRegistry SatelliteRegistry => GetService<ISatelliteRegistry>();

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Container = new Container().AddAllService(new CommandLineOptions(), new RenderOptions(1f, 1f, Color.Aqua));
        }

        protected T GetService<T>() where T : class => Container.GetInstance<T>();

        [SetUp]
        public async Task SetupAsync() => await SatelliteRegistry.InitialiseAsync(DefinitionsPath);
    }
}