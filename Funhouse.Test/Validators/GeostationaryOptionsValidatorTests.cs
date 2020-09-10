using System;
using System.IO;
using Funhouse.Models;
using Funhouse.Models.CommandLine;
using Funhouse.Test.Helper;
using Funhouse.Validators;
using NUnit.Framework;

namespace Funhouse.Test.Validators
{
    [TestFixture(TestOf = typeof(GeostationaryOptionsValidator))]
    public class GeostationaryOptionsValidatorTests : AbstractValidatorTests<GeostationaryOptionsValidator, GeostationaryOptions>
    {
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
        public void OutputNotDirectoryIfLongitudeSpecified()
        {
            var options = ValidOptions();
            options.Longitude = 147;
            
            using var state = new FileState();

            options.SourcePath = Path.Combine(state.CreateTempDirectory(), "*.jpg");
            options.OutputPath = state.CreateTempDirectory();

            VerifyFailure(
                options, 
                nameof(GeostationaryOptions.OutputPath), 
                "If multiple source files are specified with a target latitude reprojection, the output cannot be a directory.");
        }

        [Test]
        public void OutputIsFileIfLongitudeSpecified()
        {
            var options = ValidOptions();
            options.Longitude = 147;
            
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
            options.Longitude = longitude;

            VerifyFailure(options, nameof(GeostationaryOptions.Longitude));
        }

        [TestCase(-180)]
        [TestCase(180)]
        [TestCase(0.12345)]
        public void ValidLongitude(double longitude)
        {
            var options = ValidOptions();
            options.Longitude = longitude;

            VerifyNoFailure(options, nameof(GeostationaryOptions.Longitude));
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