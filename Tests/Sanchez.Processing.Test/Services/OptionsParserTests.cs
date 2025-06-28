using AwesomeAssertions.Execution;
using Sanchez.Models.CommandLine;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Configuration;
using Sanchez.Processing.Models.Options;
using Sanchez.Services;
using SixLabors.ImageSharp;

namespace Sanchez.Processing.Test.Services;

[TestFixture(TestOf = typeof(OptionsParser))]
public class OptionsParserTests
{
    [Test]
    public void PopulateGeostationary()
    {
        var options = new GeostationaryOptions
        {
            AtmosphereAmount = 0.5f,
            LongitudeDegrees = 147,
            Tint = "ff0000",
            SpatialResolution = Constants.Satellite.SpatialResolution.TwoKm,
            InterpolationType = InterpolationOptions.N,
            Force = false,
            Verbose = false,
            Quiet = true
        };

        var renderOptions = OptionsParser.Populate(options);

        using (new AssertionScope())
        {
            renderOptions.GeostationaryRender.Should().NotBeNull();

            renderOptions.GeostationaryRender!.Longitude.Should().Be(Angle.FromDegrees(147).Radians);
            renderOptions.GeostationaryRender.AtmosphereAmount.Should().Be(0.5f);
            renderOptions.InterpolationType.Should().Be(InterpolationType.NearestNeighbour);
            renderOptions.ImageSize.Should().Be(Constants.Satellite.ImageSize.TwoKm);
            renderOptions.ImageOffset.Should().Be(Constants.Satellite.Offset.TwoKm);
            renderOptions.Force.Should().BeFalse();
            renderOptions.Verbose.Should().BeFalse();
            renderOptions.Quiet.Should().BeTrue();
        }
    }

    [Test]
    public void PopulateEquirectangular()
    {
        var options = new EquirectangularOptions
        {
            AutoCrop = true,
            Tint = "ff0000",
            SpatialResolution = Constants.Satellite.SpatialResolution.FourKm,
            InterpolationType = InterpolationOptions.B,
            IntervalMinutes = 30,
            Brightness = 1.2f,
            Saturation = 0.5f,
            Force = true,
            Verbose = true,
            LongitudeRange = "-180:180",
            LatitudeRange = "-50:50"
        };

        var renderOptions = OptionsParser.Populate(options);

        using (new AssertionScope())
        {
            renderOptions.EquirectangularRender.Should().NotBeNull();

            renderOptions.EquirectangularRender!.AutoCrop.Should().BeTrue();

            renderOptions.EquirectangularRender.LatitudeRange.Should().NotBeNull();
            renderOptions.EquirectangularRender.LongitudeRange.Should().NotBeNull();

            renderOptions.EquirectangularRender.LatitudeRange!.Value.Start.Should().Be(Angle.FromDegrees(-50).Radians);
            renderOptions.EquirectangularRender.LatitudeRange!.Value.End.Should().Be(Angle.FromDegrees(50).Radians);

            renderOptions.EquirectangularRender.LongitudeRange!.Value.Start.Should().Be(Angle.FromDegrees(-180).Radians);
            renderOptions.EquirectangularRender.LongitudeRange!.Value.End.Should().Be(Angle.FromDegrees(180).Radians);

            renderOptions.SpatialResolution.Should().Be(Constants.Satellite.SpatialResolution.FourKm);
            renderOptions.Tint.Should().Be(Color.FromRgb(255, 0, 0));
            renderOptions.InterpolationType.Should().Be(InterpolationType.Bilinear);
            renderOptions.ImageSize.Should().Be(Constants.Satellite.ImageSize.FourKm);
            renderOptions.ImageOffset.Should().Be(Constants.Satellite.Offset.FourKm);
            renderOptions.Interval.Should().Be(TimeSpan.FromMinutes(30));
            renderOptions.Brightness.Should().Be(1.2f);
            renderOptions.Saturation.Should().Be(0.5f);
            renderOptions.Force.Should().BeTrue();
            renderOptions.Verbose.Should().BeTrue();
        }
    }
}