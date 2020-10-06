using System.IO;
using Extend;
using FluentValidation;
using Sanchez.Models.CommandLine;
using Sanchez.Processing.Extensions;
using Sanchez.Processing.Models;

namespace Sanchez.Validators
{
    public class CommandLineOptionsValidator<T> : AbstractValidator<T> where T : CommandLineOptions
    {
        protected CommandLineOptionsValidator()
        {
            RuleFor(o => o.Tint.FromHexString())
                .NotNull()
                .WithName("Tint")
                .WithMessage("Unable to parse tint as a hex tuple. Expected format is 5ebfff");

            RuleFor(o => o.UnderlayPath)
                .Must(path => File.Exists(path ?? Constants.DefaultUnderlayPath))
                .WithMessage(o => $"Invalid underlay path: {o.UnderlayPath}");

            RuleFor(o => o.DefinitionsPath)
                .Must(path => File.Exists(path ?? Constants.DefaultDefinitionsPath))
                .WithMessage(o => $"Invalid satellite definitions path: {o.DefinitionsPath}");

            RuleFor(o => o.SpatialResolution)
                .Must(resolution => resolution.IsIn(Constants.Satellite.SpatialResolution.TwoKm, Constants.Satellite.SpatialResolution.FourKm))
                .WithMessage($"Unsupported output spatial resolution. Valid values are: {Constants.Satellite.SpatialResolution.TwoKm}, {Constants.Satellite.SpatialResolution.FourKm}");

            ValidateTimeOptions();
        }

        /// <summary>
        ///     Validates options concerning stitching and timelapses.
        /// </summary>
        private void ValidateTimeOptions()
        {
            RuleFor(o => o.IntervalMinutes)
                .GreaterThan(0)
                .When(o => o.Timestamp != null)
                .WithMessage("Interval must be a positive value.");

            RuleFor(o => o.EndTimestamp)
                .GreaterThan(o => o.Timestamp)
                .When(o => o.Timestamp != null)
                .WithMessage("End timestamp must be greater than timestamp.");

            RuleFor(o => o.IntervalMinutes)
                .GreaterThanOrEqualTo(o => o.ToleranceMinutes)
                .When(o => o.Timestamp != null)
                .WithMessage(o => $"Interval must be greater than or equal to the tolerance of {o.ToleranceMinutes} minutes");

            RuleFor(o => o.EndTimestamp)
                .GreaterThanOrEqualTo(o => o.Timestamp)
                .When(o => o.EndTimestamp != null)
                .WithMessage("End timestamp must be empty or later than start timestamp.");

            RuleFor(o => o.ToleranceMinutes)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Tolerance must be a positive value.");
            
            // Verify that a file can be created if multiple source files are provided without a target timestamp 
            RuleFor(o => o.OutputPath)
                .Must((options, outputPath) => !File.Exists(outputPath))
                .When(o => o.MultipleSources && (o.IntervalMinutes != null || o.Timestamp == null))
                .WithMessage("The output cannot be a file if rendering multiple images.");
        }
    }
}