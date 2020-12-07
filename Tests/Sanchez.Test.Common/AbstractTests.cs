using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Sanchez.Builders;
using Sanchez.Models.CommandLine;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Angles;
using Sanchez.Processing.Models.Configuration;
using Sanchez.Processing.Models.Options;
using Sanchez.Processing.Models.Projections;
using Sanchez.Processing.Services;
using Sanchez.Processing.Services.Underlay;
using Sanchez.Processing.Test.Helper;
using Sanchez.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Sanchez.Test.Common
{
    public abstract class AbstractTests
    {
        /// <summary>
        ///     Small number for floating-point comparison tests.
        /// </summary>
        protected const double Precision = 0.000001;

        protected const string Goes16DefinitionPrefix = "GOES16_FD_CH13_20200830T033031Z.jpg";
        protected const string Goes16Filename = "GOES17_FD_CH13_20200830T033031Z.jpg";

        private static string DefinitionsPath => Path.Combine(TestContext.CurrentContext.TestDirectory, Constants.DefaultDefinitionsPath);
        protected RenderOptions RenderOptions => GetService<RenderOptions>();
        
        protected ISatelliteRegistry SatelliteRegistry => GetService<ISatelliteRegistry>();
        private IUnderlayCacheRepository UnderlayCacheRepository => GetService<IUnderlayCacheRepository>();

        private ServiceProvider ServiceProvider { get; set; }
        protected FileState State { get; set;  }

        [SetUp]
        public virtual void SetUp() => State = new FileState();

        [TearDown]
        public virtual void TearDown() => State.Dispose();


        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var options = OptionsParser.Populate(new GeostationaryOptions
            {
                Tint = "ff0000",
                InterpolationType = InterpolationOptions.B,
                SpatialResolution = Constants.Satellite.SpatialResolution.TwoKm,
                DefinitionsPath = DefinitionsPath,
                HazeAmount = 1.0f
            });

            // Build DI container
            ServiceProvider = ServiceProviderFactory.ConfigureServices(options);

            UnderlayCacheRepository.DeleteCache();
            UnderlayCacheRepository.Initialise();
        }

        protected T GetService<T>() where T : class => ServiceProvider.GetRequiredService<T>();

        [SetUp]
        public async Task SetupAsync() => await SatelliteRegistry.InitialiseAsync();

        protected async Task CreateImage(string path)
        {
            var image = new Image<Rgba32>(10, 10);
            image.Mutate(c => c.Fill(Color.Crimson));
            await image.SaveAsync(path);
        }
        
        protected static Registration EmptyRegistration(string path = "") => new Registration(path, new SatelliteDefinition("", null, null, false, 0, new Range(0, 0), new Range(0, 0)), null);
    }
}