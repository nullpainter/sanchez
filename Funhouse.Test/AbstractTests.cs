using System.IO;
using System.Threading.Tasks;
using Funhouse.Builders;
using Funhouse.Models;
using Funhouse.Models.CommandLine;
using Funhouse.Services;
using Funhouse.Services.Underlay;
using NUnit.Framework;
using SimpleInjector;

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
            var options = new RenderOptions();
            OptionsParser.Populate(new GeostationaryOptions
            {
                Tint = "ff0000",
                InterpolationType = InterpolationOptions.B,
                SpatialResolution = Constants.Satellite.SpatialResolution.TwoKm,
                HazeAmount = 1.0f
            });


            Container = new Container().AddAllService(options);

            UnderlayCacheRepository.DeleteCache();
            UnderlayCacheRepository.Initialise();
        }

        protected T GetService<T>() where T : class => Container.GetInstance<T>();

        [SetUp]
        public async Task SetupAsync() => await SatelliteRegistry.InitialiseAsync(DefinitionsPath);
    }
}