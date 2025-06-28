using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Sanchez.Builders;
using Sanchez.Models.CommandLine;
using Sanchez.Processing.Extensions.Images;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Options;
using Sanchez.Processing.Services;
using Sanchez.Processing.Services.Underlay;
using Sanchez.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Sanchez.Processing.Helpers;

namespace Sanchez.Test.Common;

public abstract class AbstractTests
{
    /// <summary>
    ///     Small number for floating-point comparison tests.
    /// </summary>
    protected const double Precision = 0.000001;

    protected const string Goes16DefinitionPrefix = "GOES16_FD_CH13_20200830T033031Z.jpg";
    protected const string Goes16Filename = "GOES17_FD_CH13_20200830T033031Z.jpg";

    private static string DefinitionsPath => Path.Combine(TestContext.CurrentContext.TestDirectory, Constants.DefaultDefinitionsPath);
    private static string ImageRootPath => Path.Combine(TestContext.CurrentContext.TestDirectory, PathHelper.ResourcePath("ImagePaths.json"));
    
    protected RenderOptions RenderOptions => GetService<RenderOptions>();
        
    protected ISatelliteRegistry SatelliteRegistry => GetService<ISatelliteRegistry>();
    private IUnderlayCacheRepository UnderlayCacheRepository => GetService<IUnderlayCacheRepository>();

    private ServiceProvider ServiceProvider { get; set; } = null!;
    protected FileState State { get; private set; } = null!;

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
            AtmosphereAmount = 1.0f,
            ImageRootPaths = ImageRootPath
        });

        // Build DI container
        ServiceProvider = ServiceProviderFactory.ConfigureServices(options);

        UnderlayCacheRepository.DeleteCache();
        UnderlayCacheRepository.Initialise();
    }

    protected T GetService<T>() where T : class => ServiceProvider.GetRequiredService<T>();

    [SetUp]
    public Task SetupAsync() => SatelliteRegistry.InitialiseAsync();

    protected static Task CreateImage(string path)
    {
        var image = new Image<Rgba32>(10, 10);
        image.AddBackgroundColour(Color.Crimson);
        return image.SaveAsync(path);
    }
}