using System.IO;
using FluentValidation;
using Funhouse.Models.CommandLine;

namespace Funhouse.Validators
{
    public class GeostationaryOptionsValidator : OptionsValidator<GeostationaryOptions>
    {
        public GeostationaryOptionsValidator()
        {
            RuleFor(o => o.Longitude)
                .InclusiveBetween(-180, 180)
                .WithMessage("Invalid target longitude; longitude must be between -180 and 180 degrees.");

            RuleFor(o => o.HazeAmount)
                .InclusiveBetween(0, 1)
                .WithMessage("Invalid haze amount; valid values are between 0.0 and 1.0.");


            // TODO also need to add a rule for the source path, yeah? Verify that the file exists, etc.
            // TODO reintroduce glob

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