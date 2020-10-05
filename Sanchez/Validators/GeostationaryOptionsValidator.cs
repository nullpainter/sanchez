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
                .When(o => o.Longitude != null)
                .WithMessage("Target timestamp must be provided if combining files.");
            
            RuleFor(o => o.Longitude)
                .InclusiveBetween(-180, 180)
                .WithMessage("Invalid target longitude; longitude must be between -180 and 180 degrees.");

            RuleFor(o => o.HazeAmount)
                .InclusiveBetween(0, 1)
                .WithMessage("Invalid haze amount; valid values are between 0.0 and 1.0.");

            // Verify that a directory can be created if multiple source files are provided without a target latitude
            RuleFor(o => o.OutputPath)
                .Must((options, outputPath) => !File.Exists(outputPath))
                .When(o => o.MultipleSources && o.Longitude == null)
                .WithMessage("If multiple source files are specified, the output must be a directory.");

            // Verify that a file can be created if multiple source files are provided with a target latitude
            RuleFor(o => o.OutputPath)
                .Must((options, outputPath) => !Directory.Exists(outputPath))
                .When(o => o.MultipleSources && o.Longitude != null)
                .WithMessage("If multiple source files are specified with a target latitude reprojection, the output cannot be a directory.");
        }
    }
}