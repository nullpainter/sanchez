using System;
using System.IO;
using Funhouse.Models;
using Funhouse.Models.CommandLine;
using Funhouse.Test.Helper;
using Funhouse.Validators;
using NUnit.Framework;

namespace Funhouse.Test.Validators
{
    [TestFixture(TestOf = typeof(EquirectangularRenderOptions))]
    public class EquirectangularOptionsValidatorTests : AbstractValidatorTests<EquirectangularOptionsValidator, EquirectangularOptions>
    {
        [Test]
        public void ValidOutput()
        {
            var options = ValidOptions();
            using var state = new FileState();
            var outputFile = state.CreateFile("foo.jpg");
            options.TargetTimestamp = DateTime.Now;

            options.SourcePath = Path.Combine(state.CreateTempDirectory(), "*.jpg");
            options.OutputPath = outputFile;

            VerifyNoFailure(options);
        }

        [Test]
        public void OutputIsDirectory()
        {
            var options = ValidOptions();
            using var state = new FileState();

            options.SourcePath = Path.Combine(state.CreateTempDirectory(), "*.jpg");
            options.OutputPath = state.CreateTempDirectory();
            
            VerifyFailure(
                options, 
                nameof(EquirectangularOptions.OutputPath), 
                "The output cannot be a directory if combining satellite imagery.");
        }

        [Test]
        public void InvalidTolerance()
        {
            var options = ValidOptions();
            options.ToleranceMinutes = -1;
            using var state = new FileState();
            
            VerifyFailure(options, nameof(EquirectangularOptions.ToleranceMinutes), "Tolerance must be a positive value.");
        }

        [Test]
        public void ValidTolerance()
        {
            var options = ValidOptions();
            options.ToleranceMinutes = 20;
            
            VerifyNoFailure(options);
        }

        [Test]
        public void TimestampRequiredForMultipleFiles()
        {
            var options = ValidOptions();
            using var state = new FileState();

            options.SourcePath = Path.Combine(state.CreateTempDirectory(), "*.jpg");
            options.OutputPath = Path.Combine(state.CreateTempDirectory(), "out.jpg");
            options.TargetTimestamp = null;
            
            VerifyFailure(
                options, 
                nameof(EquirectangularOptions.TargetTimestamp),
                "Target timestamp must be provided when processing multiple source images.");
        }

        private EquirectangularOptions ValidOptions()
        {
            return new EquirectangularOptions
            {
                Tint = "0000FF",
                SpatialResolution = Constants.Satellite.SpatialResolution.TwoKm
            };
        }
    }
}