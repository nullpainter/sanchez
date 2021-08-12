using System.IO;
using FluentValidation;
using Sanchez.Models.CommandLine;

namespace Sanchez.Validators
{
    public class GeostationaryOptionsValidator : CommandLineOptionsValidator<GeostationaryOptions>
    {
        public GeostationaryOptionsValidator()
        {
            RuleFor(o => o.Timestamp)
                .NotNull()
                .When(o => o.LongitudeDegrees != null && o.IntervalMinutes == null)
                .WithMessage("Target timestamp must be provided if combining files.");

            RuleFor(o => o.LongitudeDegrees)
                .InclusiveBetween(-180, 180)
                .WithMessage("Invalid target longitude; longitude must be between -180 and 180 degrees.");

            RuleFor(o => o.EndLongitudeDegrees)
                .InclusiveBetween(-180, 180)
                .WithMessage("Invalid end longitude; longitude must be between -180 and 180 degrees.");

            RuleFor(o => o.InverseRotation)
                .Equal(false)
                .When(o => o.EndLongitudeDegrees == null)
                .WithMessage("Inverse rotation can only be applied when sweeping through a longitude range.");

            RuleFor(o => o.HazeAmount)
                .InclusiveBetween(0, 1)
                .WithMessage("Invalid haze amount; valid values are between 0.0 and 1.0.");

            RuleFor(o => o.HazeOpacity)
                .InclusiveBetween(0, 1)
                .WithMessage("Invalid haze opacity; valid values are between 0.0 and 1.0.");

            // Verify that a directory can be created if multiple source files are provided without a target latitude
            RuleFor(o => o.OutputPath)
                .Must((_, outputPath) => !File.Exists(outputPath))
                .When(o => o.MultipleSources && o.LongitudeDegrees == null)
                .WithMessage("If multiple source files are specified, the output must be a directory.");
        }
    }
}