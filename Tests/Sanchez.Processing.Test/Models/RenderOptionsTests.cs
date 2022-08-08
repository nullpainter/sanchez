using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Projections;
using Sanchez.Test.Common;

namespace Sanchez.Processing.Test.Models;

[TestFixture(TestOf = typeof(RenderOptions))]
public class RenderOptionsTests : AbstractTests
{
    [Test]
    public void NoMultipleTargetsIfEquirectangular()
    {
        var options = EquirectangularOptions();
        options.SourcePath = "images/*";

        options.MultipleTargets.Should().BeFalse("equirectangular rendering never outputs multiple targets");
    }

    [Test]
    public void NoMultipleTargetsIfGeostationaryWithLongitude()
    {
        var options = GeostationaryOptions(Angle.FromDegrees(174));
        options.SourcePath = "images/*";

        options.MultipleTargets.Should().BeFalse("geostationary render with multiple sources and longitude should result in a single target");
    }

    [Test]
    public void NoMultipleTargetsIfSingleSource()
    {
        var options = GeostationaryOptions();
        options.SourcePath = "image.jpg";

        options.MultipleTargets.Should().BeFalse("a single source always results in a single target");
    }


    [Test]
    public void MultipleTargetsIfGeostationary()
    {
        var options = GeostationaryOptions();
        options.SourcePath = "images/*";

        options.MultipleTargets.Should().BeTrue("geostationary render with multiple sources and no longitude should result in multiple targets");
    }

    [TestCase("source/*", ExpectedResult = true)]
    [TestCase("image.jpg", ExpectedResult = false)]
    [TestCase("missingDirectory", ExpectedResult = false)]
    public bool MultipleSources(string sourcePath)
    {
        var options = EquirectangularOptions();
        options.SourcePath = sourcePath;

        return options.MultipleSources;
    }

    [Test]
    public void MultipleSourcesIfDirectory()
    {
        var directory = State.CreateTempDirectory();

        var options = EquirectangularOptions();
        options.SourcePath = directory;

        options.MultipleSources.Should().BeTrue();
    }

    [Test]
    public void EquirectangularProjection()
    {
        var options = EquirectangularOptions();
        options.Projection.Should().Be(ProjectionType.Equirectangular);
    }

    [Test]
    public void GeostationaryProjection()
    {
        var options = GeostationaryOptions();
        options.Projection.Should().Be(ProjectionType.Geostationary);
    }

    [Test]
    public void StitchIfEquirectangular()
    {
        var options = EquirectangularOptions();
        options.StitchImages.Should().BeTrue("equirectangular rendering always stitches images");
    }

    [Test]
    public void StitchIfGeostationaryWithLongitude()
    {
        var options = GeostationaryOptions(Angle.FromDegrees(17));
        options.StitchImages.Should().BeTrue("geostationary rendering with a longitude should stitch images");
    }

    [Test]
    public void NoStitchIfGeostationaryAndNoLatitude()
    {
        var options = GeostationaryOptions();
        options.StitchImages.Should().BeFalse("geostationary rendering without a longitude doesn't stitch images");
    }

    private static RenderOptions GeostationaryOptions(Angle? longitude = null, Angle? endLongitude = null)
    {
        return new()
        {
            GeostationaryRender = new GeostationaryRenderOptions(longitude, endLongitude, false, 1.0f, 0.9f)
        };
    }

    private static RenderOptions EquirectangularOptions()
    {
        return new()
        {
            EquirectangularRender = new EquirectangularRenderOptions(false, false, true)
        };
    }
}