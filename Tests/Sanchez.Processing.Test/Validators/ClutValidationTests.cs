using Sanchez.Models.CommandLine;
using Sanchez.Processing.Models;
using Sanchez.Validators;

namespace Sanchez.Processing.Test.Validators;

[TestFixture(TestOf = typeof(CommandLineOptionsValidator<>))]
public class ClutValidationTests : AbstractValidatorTests<GeostationaryOptionsValidator, GeostationaryOptions>
{
    [Test]
    public void ValidGradientPath()
    {
        var options = ValidOptions();
        VerifyNoFailure(options, nameof(GeostationaryOptions.GradientPath));
    }

    [Test]
    public void InvalidGradientPath()
    {
        var options = ValidOptions();
        options.GradientPath = "Resources/Gradients/Polkadot-Taupe.json";

        VerifyFailure(options, nameof(GeostationaryOptions.GradientPath));
    }


    [Test]
    public void MinGreaterThanMax()
    {
        var options = ValidOptions();
        options.ClutRange = "0.4-0.2";

        VerifyFailure(options, nameof(GeostationaryOptions.ClutRange));
    }

    [Test]
    public void InvalidMinValue()
    {
        var options = ValidOptions();
        options.ClutRange = "2-4";

        VerifyFailure(options, nameof(GeostationaryOptions.ClutRange));
    }

    [Test]
    public void InvalidMaxValue()
    {
        var options = ValidOptions();
        options.ClutRange = "0-1.1";

        VerifyFailure(options, nameof(GeostationaryOptions.ClutRange));
    }

    private static GeostationaryOptions ValidOptions()
    {
        var options = new GeostationaryOptions
        {
            GradientPath = "Resources/Gradients/Red-Blue.json",
            ClutRange = "0-1",
            SpatialResolution = Constants.Satellite.SpatialResolution.FourKm,
            Tint = "#ff0000"
        };

        return options;
    }
}