using System.IO;
using FluentValidation;
using Sanchez.Models.CommandLine;

namespace Sanchez.Validators
{
    public class EquirectangularOptionsValidator : CommandLineOptionsValidator<EquirectangularOptions>
    {
        public EquirectangularOptionsValidator()
        {
            // Verify that a file can be created if multiple source files are provided without a target timestamp 
            RuleFor(o => o.OutputPath)
                .Must((options, outputPath) => !File.Exists(outputPath))
                .When(o => o.MultipleSources && (o.IntervalMinutes != null || o.Timestamp == null))
                .WithMessage("The output cannot be a file if rendering multiple images.");

            RuleFor(o => o.EndTimestamp)
                .GreaterThan(o => o.Timestamp)
                .When(o => o.Timestamp != null)
                .WithMessage("End timestamp must be greater than timestamp.");
        }
    }
}