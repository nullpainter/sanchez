using System;
using System.IO;
using NUnit.Framework;
using Sanchez.Models.CommandLine;
using Sanchez.Processing.Models;
using Sanchez.Processing.Test.Helper;
using Sanchez.Validators;

namespace Sanchez.Processing.Test.Validators
{
    [TestFixture(TestOf = typeof(EquirectangularRenderOptions))]
    public class EquirectangularOptionsValidatorTests : AbstractValidatorTests<EquirectangularOptionsValidator, EquirectangularOptions>
    {
        [Test]
        public void ValidOutputStitchNoBatch()
        {
            var options = ValidOptions();
            using var state = new FileState();
            var outputFile = state.CreateFile("foo.jpg");
            options.Timestamp = DateTime.Now;

            options.SourcePath = Path.Combine(state.CreateTempDirectory(), "*.jpg");
            options.OutputPath = outputFile;

            VerifyNoFailure(options);
        }

        [Test]
        public void ValidOutputStitchAndBatch()
        {
            var options = ValidOptions();
            using var state = new FileState();
            options.Timestamp = DateTime.Now;
            options.IntervalMinutes = 30;

            options.SourcePath = Path.Combine(state.CreateTempDirectory(), "*.jpg");
            options.OutputPath = state.CreateTempDirectory();

            VerifyNoFailure(options);
        }

        [Test]
        public void ValidOutputNoStitch()
        {
            var options = ValidOptions();
            using var state = new FileState();

            options.SourcePath = Path.Combine(state.CreateTempDirectory(), "*.jpg");
            options.OutputPath = state.CreateTempDirectory();

            VerifyNoFailure(options);
        }

        [Test]
        public void InvalidInterval()
        {
            var options = ValidOptions();
            options.IntervalMinutes = -1;
            options.Timestamp = DateTime.Now;

            VerifyFailure(options, nameof(CommandLineOptions.IntervalMinutes));
        }

        [Test]
        public void ValidInterval()
        {
            var options = ValidOptions();
            options.IntervalMinutes = options.ToleranceMinutes;
            options.Timestamp = DateTime.Now;

            VerifyNoFailure(options);
        }
        
        [Test]
        public void IntervalLessThanTolerance()
        {
            var options = ValidOptions();
            options.IntervalMinutes = options.ToleranceMinutes - 1;
            options.Timestamp = DateTime.Now;

            VerifyFailure(options, nameof(CommandLineOptions.IntervalMinutes));
        }

        [Test]
        public void InvalidTolerance()
        {
            var options = ValidOptions();
            options.ToleranceMinutes = -1;
            options.Timestamp = DateTime.Now;

            VerifyFailure(options, nameof(CommandLineOptions.ToleranceMinutes));
        }

        [Test]
        public void ValidTolerance()
        {
            var options = ValidOptions();
            options.IntervalMinutes = 20;

            VerifyNoFailure(options);
        }

        private EquirectangularOptions ValidOptions()
        {
            return new EquirectangularOptions
            {
                Tint = "0000FF",
                ToleranceMinutes = 30,
                SpatialResolution = Constants.Satellite.SpatialResolution.TwoKm
            };
        }
    }
}