using Extend;
using FluentValidation;
using Sanchez.Models.CommandLine;
using Sanchez.Processing.Extensions;
using Sanchez.Processing.Models;

namespace Sanchez.Validators;

public class CommandLineOptionsValidator<T> : AbstractValidator<T> where T : CommandLineOptions
{
    protected CommandLineOptionsValidator()
    {
        RuleFor(o => o.Tint.FromHexString())
            .NotNull()
            .WithName("Tint")
            .WithMessage("Unable to parse tint as a hex tuple. Expected format is 5ebfff");

        RuleFor(o => o.UnderlayPath)
            .Must(path => File.Exists(path ?? Constants.DefaultUnderlayPath))
            .WithMessage(o => $"Invalid underlay path: {o.UnderlayPath}");

        RuleFor(o => o.DefinitionsPath)
            .Must(path => File.Exists(path ?? Constants.DefaultDefinitionsPath))
            .WithMessage(o => $"Invalid satellite definitions path: {o.DefinitionsPath}");

        RuleFor(o => o.SpatialResolution)
            .Must(resolution => resolution.IsIn(
                Constants.Satellite.SpatialResolution.HalfKm,
                Constants.Satellite.SpatialResolution.OneKm,
                Constants.Satellite.SpatialResolution.TwoKm,
                Constants.Satellite.SpatialResolution.FourKm))
            .WithMessage(
                $"Unsupported output spatial resolution. Valid values are: {Constants.Satellite.SpatialResolution.HalfKm}, {Constants.Satellite.SpatialResolution.OneKm}, {Constants.Satellite.SpatialResolution.TwoKm}, {Constants.Satellite.SpatialResolution.FourKm}");

        RuleFor(o => o.OutputFormat)
            .Must(format => format == null || format.ToLower().IsIn(Constants.SupportedExtensions.Png, Constants.SupportedExtensions.Jpg))
            .WithMessage($"Unsupported output format. Valid values are: {Constants.SupportedExtensions.Png}, {Constants.SupportedExtensions.Jpg}");

        ValidateTimeOptions();
        ValidateOverlayOptions();
    }

    private void ValidateOverlayOptions()
    {
        RuleFor(o => o.GradientPath)
            .Must(path => File.Exists(path ?? Constants.DefaultGradientPath))
            .When(o => o.ClutRange != null)
            .WithMessage(o => $"Invalid gradient path: {o.GradientPath}");

        RuleFor(o => o.ClutRange)
            .Must(r =>
            {
                if (r == null) return true;

                var range = r.Split('-');
                if (range.Length != 2) return false;

                if (!float.TryParse(range[0], out var minIntensity)) return false;
                if (!float.TryParse(range[1], out var maxIntensity)) return false;

                if (minIntensity < 0) return false;
                if (maxIntensity > 1) return false;

                if (minIntensity >= maxIntensity) return false;

                return true;
            })
            .When(r => r != null)
            .WithMessage("Invalid intensity range; expected format is min-max; e.g. 0.0 to 1.0");
    }

    /// <summary>
    ///     Validates options concerning stitching and timelapses.
    /// </summary>
    private void ValidateTimeOptions()
    {
        RuleFor(o => o.IntervalMinutes)
            .GreaterThan(0)
            .When(o => o.Timestamp != null)
            .WithMessage("Interval must be a positive value.");

        RuleFor(o => o.EndTimestamp)
            .GreaterThan(o => o.Timestamp)
            .When(o => o.Timestamp != null)
            .WithMessage("End timestamp must be greater than timestamp.");

        RuleFor(o => o.EndTimestamp)
            .GreaterThanOrEqualTo(o => o.Timestamp)
            .When(o => o.EndTimestamp != null && o.Timestamp != null)
            .WithMessage("End timestamp must be empty or later than start timestamp.");

        RuleFor(o => o.ToleranceMinutes)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Tolerance must be a positive value.");

        // Verify that a file can be created if multiple source files are provided without a target timestamp 
        RuleFor(o => o.OutputPath)
            .Must((_, outputPath) => !File.Exists(outputPath))
            .When(o => o.MultipleSources && (o.IntervalMinutes != null || o.Timestamp == null))
            .WithMessage("The output cannot be a file if rendering multiple images.");
    }
}