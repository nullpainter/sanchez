using System;
using Extend;
using FluentValidation;
using Sanchez.Models.CommandLine;
using Sanchez.Processing.Helpers;
using Sanchez.Processing.Models;

namespace Sanchez.Validators
{
    public class EquirectangularOptionsValidator : CommandLineOptionsValidator<EquirectangularOptions>
    {
        public EquirectangularOptionsValidator()
        {
            RuleFor(o => o.AutoCrop)
                .Must(crop => !crop)
                .When(o => o.NoCrop)
                .WithMessage(o => "Automatic cropping cannot be performed if no cropping is specified");

            RuleFor(o => o.AutoCrop)
                .Must(crop => !crop)
                .When(o => o.LatitudeRange != null || o.LongitudeRange != null)
                .WithMessage(o => "Automatic cropping cannot be performed if manual crop bounds are specified`");

            RuleFor(o => o.LatitudeRange)
                .Must(ValidLatitude)
                .When(o => !o.LatitudeRange.IsNullOrEmpty())
                .WithMessage(o => "Unable to parse latitude crop range. Expected format is min:max; e.g. -174:180"); // TODO replace with NZ range

            RuleFor(o => o.LongitudeRange)
                .Must(ValidRange)
                .When(o => !o.LongitudeRange.IsNullOrEmpty())
                .WithMessage(o => "Unable to parse longitude crop range. Expected format is min:max; e.g. -174:180"); // TODO replace with NZ range
        }

        private static bool ValidLatitude(string? range)
        {
            var parsedRange = RangeHelper.ParseRange(range);
            if (parsedRange == null) return false;

            // TODO is this back-to-front? Need to test plz
            return parsedRange.Value.Start >= -90 && parsedRange.Value.End <= 90;
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
}