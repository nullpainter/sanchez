using System.IO;
using FluentValidation;
using Sanchez.Models.CommandLine;

namespace Sanchez.Validators
{
    public class EquirectangularOptionsValidator : OptionsValidator<EquirectangularOptions>
    {
        public EquirectangularOptionsValidator()
        {
            RuleFor(o => o.TargetTimestamp)
                .NotNull()
                .When(o => o.MultipleSources && o.Mode == EquirectangularMode.Stitch)
                .WithMessage("Target timestamp must be provided when processing multiple source images in stitch mode.");

            RuleFor(o => o.ToleranceMinutes)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Tolerance must be a positive value.");

            // Verify that a file can be created if multiple source files are provided with a target latitude
            RuleFor(o => o.OutputPath)
                .Must((options, outputPath) => !Directory.Exists(outputPath))
                .When(o => o.MultipleSources && o.Mode == EquirectangularMode.Stitch)
                .WithMessage("The output cannot be a directory if rendering a single image.");

            // Verify that a file can be created if multiple source files are provided with a target latitude
            RuleFor(o => o.OutputPath)
                .Must((options, outputPath) => !File.Exists(outputPath))
                .When(o => o.MultipleSources && o.Mode == EquirectangularMode.Batch)
                .WithMessage("The output cannot be a file if rendering multiple images.");

            // RuleFor(o => o.AutoCrop)
            //     .Must(a => !a)
            //     .When(o => o.Extents != null)
            //     .WithMessage("Autocrop not available if extents are provided.");
            //
            // // Validate extents structure
            // RuleFor(o => o.Extents)
            //     .Must(e => ExtentsHelper.ParseExtentsString(e) != null)
            //     .When(o => o.Extents != null)
            //     .WithMessage("Unable to parse extents. Expected format is minlat:maxlat:minlon:maxlon; e.g. -100:100:-45:45");
            //
            // // Validate extents range
            // RuleFor(o => o.Extents)
            //     .Must(e =>
            //     {
            //         var extents = ExtentsHelper.ParseExtentsString(e);
            //         if (extents == null) return false;
            //
            //         var latitudeExtents = extents.Value.Latitude;
            //         var longitudeExtents = extents.Value.Longitude;
            //
            //         if (Angle.FromDegrees(latitudeExtents.Start).Degrees < -90) return false;
            //         if (Angle.FromDegrees(latitudeExtents.End).Degrees > 90) return false;
            //         if (Angle.FromDegrees(longitudeExtents.Start).Degrees < -180) return false;
            //         if (Angle.FromDegrees(longitudeExtents.End).Degrees > 180) return false;
            //
            //         return true;
            //     })
            //     .When(o => o.Extents != null && ExtentsHelper.ParseExtentsString(o.Extents) != null)
            //     .WithMessage("Invalid extent range. Latitude must be between -90 and 90 degrees, and longitude must be between -180 and 180 degrees.");
        }
    }
}