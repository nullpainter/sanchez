using System.IO;
using FluentValidation;
using Funhouse.Models.CommandLine;

namespace Funhouse.Validators
{
    public class EquirectangularOptionsValidator : OptionsValidator<EquirectangularOptions>
    {
        public EquirectangularOptionsValidator()
        {
            RuleFor(o => o.TargetTimestamp)
                .NotNull()
                .When(o => o.MultipleSources)
                .WithMessage("Target timestamp must be provided when processing multiple source images.");

            RuleFor(o => o.ToleranceMinutes)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Tolerance must be a positive value.");

            // Verify that a file can be created if multiple source files are provided with a target latitude
            RuleFor(o => o.OutputPath)
                .Must((options, outputPath) => !Directory.Exists(outputPath))
                .When(o => o.MultipleSources)
                .WithMessage("The output cannot be a directory if combining satellite imagery.");
        }
    }
}