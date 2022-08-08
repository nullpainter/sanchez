using Extend;
using FluentValidation;
using Sanchez.Models.CommandLine;
using Sanchez.Processing.Helpers;
using Sanchez.Processing.Models;

namespace Sanchez.Validators;

public class EquirectangularOptionsValidator : CommandLineOptionsValidator<EquirectangularOptions>
{
    public EquirectangularOptionsValidator()
    {
        RuleFor(o => o.AutoCrop)
            .Must(crop => !crop)
            .When(o => o.NoCrop)
            .WithMessage(_ => "Automatic cropping cannot be performed if no cropping is specified");

        RuleFor(o => o.AutoCrop)
            .Must(crop => !crop)
            .When(o => o.LatitudeRange != null || o.LongitudeRange != null)
            .WithMessage(_ => "Automatic cropping cannot be performed if manual crop bounds are specified");

        RuleFor(o => o.LatitudeRange)
            .Must(ValidLatitude)
            .When(o => !o.LatitudeRange.IsNullOrEmpty())
            .WithMessage(_ => "Unable to parse latitude crop range. Expected format is min:max; e.g. -33.6:-48");

        RuleFor(o => o.LongitudeRange)
            .Must(ValidRange)
            .When(o => !o.LongitudeRange.IsNullOrEmpty())
            .WithMessage(_ => "Unable to parse longitude crop range. Expected format is min:max; e.g. 165.1:179.3");
    }

    private static bool ValidLatitude(string? range)
    {
        var parsedRange = RangeHelper.ParseRange(range);
        if (parsedRange == null) return false;

        return Angle.FromRadians(parsedRange.Value.Start).Degrees <= 90 && Angle.FromRadians(parsedRange.Value.End).Degrees >= -90;
    }

    private static bool ValidRange(string? range)
    {
        var parsedRange = RangeHelper.ParseRange(range);
        if (parsedRange == null) return false;

        // Ensure that the start angle isn't the same as the end
        var unwrappedRange = parsedRange.Value.UnwrapLongitude();
        return Math.Abs(unwrappedRange.Start - unwrappedRange.End) > Constants.FloatingTolerance;
    }
}