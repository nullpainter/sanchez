using System.IO;
using System.Threading.Tasks;
using Funhouse.Builders;
using Funhouse.Factories;
using Funhouse.Models;
using Funhouse.Models.CommandLine;
using Funhouse.Services;
using Funhouse.Services.Underlay;
using NUnit.Framework;
using SimpleInjector;
using ProjectionOptions = Funhouse.Models.CommandLine.ProjectionOptions;

namespace Funhouse.Test
{
    public abstract class AbstractTests
    {
        /// <summary>
        ///     Small number for floating-point comparison tests.
        /// </summary>
        protected const double Precision = 0.000001;

        protected const string Goes16DefinitionPrefix = "GOES16_FD_CH13_";
        protected const string Goes17DefinitionPrefix = "GOES17_FD_CH13_";
        

        private Container Container { get; set; }

        private static string DefinitionsPath => Path.Combine(TestContext.CurrentContext.TestDirectory, Constants.DefinitionsPath);

        protected ISatelliteRegistry SatelliteRegistry => GetService<ISatelliteRegistry>();
        protected IUnderlayCacheRepository UnderlayCacheRepository => GetService<IUnderlayCacheRepository>();

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var options = new CommandLineOptions
            {
                Tint = "ff0000",
                InterpolationType = InterpolationOptions.B,
                ProjectionType = ProjectionOptions.E,
                SpatialResolution = Constants.Satellite.SpatialResolution.TwoKm
            };
            
            var renderOptions = RenderOptionFactory.ToRenderOptions(options);
            
            Container = new Container().AddAllService(options, renderOptions);

            UnderlayCacheRepository.DeleteCache();
            UnderlayCacheRepository.Initialise();
        }

        protected T GetService<T>() where T : class => Container.GetInstance<T>();

        [SetUp]
        public async Task SetupAsync() => await SatelliteRegistry.InitialiseAsync(DefinitionsPath);
    }
}