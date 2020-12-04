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
                .WithMessage(o => "Automatic cropping cannot be performed if manual crop bounds are specified");

            RuleFor(o => o.StartLongitudeDegrees)
                .Must(ValidateLongitude)
                .When(o => o.StartLongitudeDegrees != null)
                .WithMessage(o => "Start longitude must be between -180 and 180 degrees");
            
            RuleFor(o => o.LatitudeRange)
                .Must(ValidLatitudeRange)
                .When(o => !o.LatitudeRange.IsNullOrEmpty())
                .WithMessage(o => "Unable to parse latitude crop range. Expected format is min:max; e.g. -33.6:-48");

            RuleFor(o => o.LongitudeRange)
                .Must(ValidateLongitudeRange)
                .When(o => !o.LongitudeRange.IsNullOrEmpty())
                .WithMessage(o => "Unable to parse longitude crop range. Expected format is min:max; e.g. 165.1:179.3"); 
        }

        private static bool ValidateLongitude(double? longitude) => longitude >= -180 && longitude <= 180;

        private static bool ValidLatitudeRange(string? range)
        {
            var parsedRange = RangeHelper.ParseRange(range);
            if (parsedRange == null) return false;

            // TODO is this back-to-front? Need to test plz
            return parsedRange.Value.Start >= -90 && parsedRange.Value.End <= 90;
        }

        private static bool ValidateLongitudeRange(string? range)
        {
            var parsedRange = RangeHelper.ParseRange(range);
            if (parsedRange == null) return false;

            // Ensure that the start angle isn't the same as the end
            var unwrappedRange = parsedRange.Value.UnwrapLongitude();
            return Math.Abs(unwrappedRange.Start - unwrappedRange.End) > Constants.FloatingTolerance;
        }
    }
}