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
        }
    }
}