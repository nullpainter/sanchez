using Sanchez.Models.CommandLine;
using Sanchez.Processing.Models;
using Sanchez.Validators;

namespace Sanchez.Processing.Test.Validators;

[TestFixture(TestOf = typeof(GeostationaryOptionsValidator))]
public class GeostationaryOptionsValidatorTests : AbstractValidatorTests<GeostationaryOptionsValidator, GeostationaryOptions>
{
    [Test]
    public void TimestampRequiredWithLongitude()
    {
        var options = ValidOptions();
        options.Timestamp = null;
        options.LongitudeDegrees = 174;

        VerifyFailure(options, nameof(GeostationaryOptions.Timestamp));
    }

    [Test]
    public void TimestampAndLongitude()
    {
        var options = ValidOptions();
        options.Timestamp = DateTime.Now;
        options.LongitudeDegrees = 174;

        VerifyNoFailure(options, nameof(GeostationaryOptions.Timestamp));
    }

    [Test]
    public void InverseRotation()
    {
        var options = ValidOptions();
        options.Timestamp = DateTime.Now;
        options.LongitudeDegrees = 174;
        options.EndLongitudeDegrees = 130;
        options.InverseRotation = true;

        VerifyNoFailure(options, nameof(GeostationaryOptions.InverseRotation));
    }

    [Test]
    public void NoInverseRotationIfNoTimestamp()
    {
        var options = ValidOptions();
        options.Timestamp = DateTime.Now;
        options.LongitudeDegrees = 174;
        options.InverseRotation = true;

        VerifyFailure(options, nameof(GeostationaryOptions.InverseRotation));
    }

    [TestCase(-0.1f)]
    [TestCase(-1.1f)]
    public void InvalidAtmosphere(float atmosphere)
    {
        var options = ValidOptions();
        options.AtmosphereAmount = atmosphere;

        VerifyFailure(options, nameof(GeostationaryOptions.AtmosphereAmount));
    }

    [TestCase(0.0f)]
    [TestCase(0.1f)]
    [TestCase(1.0f)]
    public void ValidAtmosphere(float atmosphere)
    {
        var options = ValidOptions();
        options.AtmosphereAmount = atmosphere;

        VerifyNoFailure(options, nameof(GeostationaryOptions.UnderlayPath));
    }

    [TestCase(-0.1f)]
    [TestCase(-1.1f)]
    public void InvalidAtmosphereOpacity(float opacity)
    {
        var options = ValidOptions();
        options.AtmosphereOpacity = opacity;

        VerifyFailure(options, nameof(GeostationaryOptions.AtmosphereOpacity));
    }

    [TestCase(0.0f)]
    [TestCase(0.1f)]
    [TestCase(1.0f)]
    public void ValidAtmosphereOpacity(float opacity)
    {
        var options = ValidOptions();
        options.AtmosphereOpacity = opacity;

        VerifyNoFailure(options, nameof(GeostationaryOptions.UnderlayPath));
    }


    [Test]
    public void MissingUnderlay()
    {
        var options = ValidOptions();
        options.UnderlayPath = $"{Guid.NewGuid()}";

        VerifyFailure(options, nameof(GeostationaryOptions.UnderlayPath));
    }

    [TestCase("0")]
    [TestCase("3")]
    public void InvalidSpatialResolution(string spatialResolution)
    {
        var options = ValidOptions();
        options.SpatialResolution = spatialResolution;

        VerifyFailure(options, nameof(GeostationaryOptions.SpatialResolution));
    }

    [TestCase(Constants.Satellite.SpatialResolution.HalfKm)]
    [TestCase(Constants.Satellite.SpatialResolution.OneKm)]
    [TestCase(Constants.Satellite.SpatialResolution.TwoKm)]
    [TestCase(Constants.Satellite.SpatialResolution.FourKm)]
    public void ValidSpatialResolution(string spatialResolution)
    {
        var options = ValidOptions();
        options.SpatialResolution = spatialResolution;

        VerifyNoFailure(options, nameof(GeostationaryOptions.SpatialResolution));
    }

    [Test]
    public void OutputNotFileIfMultipleSource()
    {
        var options = ValidOptions();
        var outputFile = State.CreateFile("foo.jpg");

        options.SourcePath = Path.Combine(State.CreateTempDirectory(), "*.jpg");
        options.OutputPath = outputFile;

        VerifyFailure(
            options,
            nameof(GeostationaryOptions.OutputPath),
            "If multiple source files are specified, the output must be a directory.");
    }

    [Test]
    public void OutputDirectoryIfMultipleSources()
    {
        var options = ValidOptions();

        options.SourcePath = Path.Combine(State.CreateTempDirectory(), "*.jpg");
        options.OutputPath = State.CreateTempDirectory();

        VerifyNoFailure(options);
    }

    [Test]
    public void OutputIsFileIfLongitudeSpecified()
    {
        var options = ValidOptions();
        options.LongitudeDegrees = 147;
        options.Timestamp = DateTime.Now;

        options.SourcePath = Path.Combine(State.CreateTempDirectory(), "*.jpg");
        options.OutputPath = Path.Combine(State.CreateTempDirectory(), "out.jpg");

        VerifyNoFailure(options);
    }

    [TestCase(-181)]
    [TestCase(181)]
    public void InvalidLongitude(double longitude)
    {
        var options = ValidOptions();
        options.LongitudeDegrees = longitude;

        VerifyFailure(options, nameof(GeostationaryOptions.LongitudeDegrees));
    }

    [TestCase(-180)]
    [TestCase(180)]
    [TestCase(0.12345)]
    public void ValidLongitude(double longitude)
    {
        var options = ValidOptions();
        options.LongitudeDegrees = longitude;

        VerifyNoFailure(options, nameof(GeostationaryOptions.LongitudeDegrees));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("bananas")]
    public void InvalidTint(string tint)
    {
        var options = ValidOptions();
        options.Tint = tint;

        VerifyFailure(options, "Tint");
    }

    [TestCase("#ff0000")]
    [TestCase("ff0000")]
    [TestCase("00112233")]
    public void ValidTint(string tint)
    {
        var options = ValidOptions();
        options.Tint = tint;

        VerifyNoFailure(options, "Tint");
    }

    private static GeostationaryOptions ValidOptions()
    {
        return new GeostationaryOptions
        {
            Tint = "0000FF",
            SpatialResolution = Constants.Satellite.SpatialResolution.TwoKm
        };
    }
}