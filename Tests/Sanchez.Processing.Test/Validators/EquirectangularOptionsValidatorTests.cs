using System;
using System.IO;
using NUnit.Framework;
using Sanchez.Models.CommandLine;
using Sanchez.Processing.Models;
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
            var outputFile = State.CreateFile("foo.jpg");
            options.Timestamp = DateTime.Now;

            options.SourcePath = Path.Combine(State.CreateTempDirectory(), "*.jpg");
            options.OutputPath = outputFile;

            VerifyNoFailure(options);
        }

        [Test]
        public void ValidOutputStitchAndBatch()
        {
            var options = ValidOptions();
            options.Timestamp = DateTime.Now;
            options.IntervalMinutes = 30;

            options.SourcePath = Path.Combine(State.CreateTempDirectory(), "*.jpg");
            options.OutputPath = State.CreateTempDirectory();

            VerifyNoFailure(options);
        }

        [Test]
        public void ValidOutputNoStitch()
        {
            var options = ValidOptions();

            options.SourcePath = Path.Combine(State.CreateTempDirectory(), "*.jpg");
            options.OutputPath = State.CreateTempDirectory();

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

        [Test]
        public void AutoCrop()
        {
            var options = ValidOptions();
            options.AutoCrop = true;

            VerifyNoFailure(options);
        }

        [Test]
        public void NoCrop()
        {
            var options = ValidOptions();
            options.NoCrop = true;

            VerifyNoFailure(options);
        }

        [Test]
        public void InvalidCropCombination()
        {
            var options = ValidOptions();
            options.NoCrop = true;
            options.AutoCrop = true;

            VerifyFailure(options, nameof(EquirectangularOptions.AutoCrop));
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