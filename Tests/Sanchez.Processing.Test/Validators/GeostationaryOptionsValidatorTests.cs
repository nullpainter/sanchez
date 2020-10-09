using System;
using System.IO;
using NUnit.Framework;
using Sanchez.Models.CommandLine;
using Sanchez.Processing.Models;
using Sanchez.Processing.Test.Helper;
using Sanchez.Validators;

namespace Sanchez.Processing.Test.Validators
{
    [TestFixture(TestOf = typeof(GeostationaryOptionsValidator))]
    public class GeostationaryOptionsValidatorTests : AbstractValidatorTests<GeostationaryOptionsValidator, GeostationaryOptions>
    {
        [Test]
        public void TimestampRequiredWithLongitude()
        {
            var options = ValidOptions();
            options.Timestamp = null;
            options.LongitudeDegrees = 174;

            VerifyFailure(options, nameof(GeostationaryOptions.Timestamp));
        }

        [Test]
        public void TimestampAndLongitude()
        {
            var options = ValidOptions();
            options.Timestamp = DateTime.Now;
            options.LongitudeDegrees = 174;

           VerifyNoFailure(options, nameof(GeostationaryOptions.Timestamp)); 
        }

        [TestCase(-0.1f)]
        [TestCase(-1.1f)]
        public void InvalidHaze(float haze)
        {
            var options = ValidOptions();
            options.HazeAmount = haze;

            VerifyFailure(options, nameof(GeostationaryOptions.HazeAmount));
        }

        [TestCase(0.0f)]
        [TestCase(0.1f)]
        [TestCase(1.0f)]
        public void ValidHaze(float haze)
        {
            var options = ValidOptions();
            options.HazeAmount = haze;

            VerifyNoFailure(options, nameof(GeostationaryOptions.UnderlayPath));
        }

        [Test]
        public void MissingUnderlay()
        {
            var options = ValidOptions();
            options.UnderlayPath = $"{Guid.NewGuid()}";

            VerifyFailure(options, nameof(GeostationaryOptions.UnderlayPath));
        }

        [TestCase(0)]
        [TestCase(3)]
        public void InvalidSpatialResolution(int spatialResolution)
        {
            var options = ValidOptions();
            options.SpatialResolution = spatialResolution;

            VerifyFailure(options, nameof(GeostationaryOptions.SpatialResolution));
        }

        [TestCase(Constants.Satellite.SpatialResolution.TwoKm)]
        [TestCase(Constants.Satellite.SpatialResolution.FourKm)]
        public void ValidSpatialResolution(int spatialResolution)
        {
            var options = ValidOptions();
            options.SpatialResolution = spatialResolution;

            VerifyNoFailure(options, nameof(GeostationaryOptions.SpatialResolution));
        }

        [Test]
        public void OutputNotFileIfMultipleSource()
        {
            var options = ValidOptions();
            using var state = new FileState();
            var outputFile = state.CreateFile("foo.jpg");

            options.SourcePath = Path.Combine(state.CreateTempDirectory(), "*.jpg");
            options.OutputPath = outputFile;

            VerifyFailure(
                options,
                nameof(GeostationaryOptions.OutputPath),
                "If multiple source files are specified, the output must be a directory.");
        }

        [Test]
        public void OutputDirectoryIfMultipleSources()
        {
            var options = ValidOptions();
            using var state = new FileState();

            options.SourcePath = Path.Combine(state.CreateTempDirectory(), "*.jpg");
            options.OutputPath = state.CreateTempDirectory();

            VerifyNoFailure(options);
        }

        [Test]
        public void OutputIsFileIfLongitudeSpecified()
        {
            var options = ValidOptions();
            options.LongitudeDegrees = 147;
            options.Timestamp = DateTime.Now;

            using var state = new FileState();

            options.SourcePath = Path.Combine(state.CreateTempDirectory(), "*.jpg");
            options.OutputPath = Path.Combine(state.CreateTempDirectory(), "out.jpg");

            VerifyNoFailure(options);
        }

        [TestCase(-181)]
        [TestCase(181)]
        public void InvalidLongitude(double longitude)
        {
            var options = ValidOptions();
            options.LongitudeDegrees = longitude;

            VerifyFailure(options, nameof(GeostationaryOptions.LongitudeDegrees));
        }

        [TestCase(-180)]
        [TestCase(180)]
        [TestCase(0.12345)]
        public void ValidLongitude(double longitude)
        {
            var options = ValidOptions();
            options.LongitudeDegrees = longitude;

            VerifyNoFailure(options, nameof(GeostationaryOptions.LongitudeDegrees));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("bananas")]
        public void InvalidTint(string tint)
        {
            var options = ValidOptions();
            options.Tint = tint;

            VerifyFailure(options, "Tint");
        }

        [TestCase("#ff0000")]
        [TestCase("ff0000")]
        [TestCase("00112233")]
        public void ValidTint(string tint)
        {
            var options = ValidOptions();
            options.Tint = tint;

            VerifyNoFailure(options, "Tint");
        }

        private GeostationaryOptions ValidOptions()
        {
            return new GeostationaryOptions
            {
                Tint = "0000FF",
                SpatialResolution = Constants.Satellite.SpatialResolution.TwoKm
            };
        }
    }
}