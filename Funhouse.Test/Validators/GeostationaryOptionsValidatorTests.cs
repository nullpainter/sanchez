using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Funhouse.Models;
using Funhouse.Models.CommandLine;
using Funhouse.Test.Helper;
using Funhouse.Validators;
using NUnit.Framework;

namespace Funhouse.Test.Validators
{
    [TestFixture(TestOf = typeof(GeostationaryOptionsValidator))]
    public class GeostationaryOptionsValidatorTests
    {
        [TestCase(-0.1f)]
        [TestCase(-1.1f)]
        public void InvalidHaze(float haze)
        {
            var options = ValidOptions();
            options.HazeAmount = haze;

            ValidateFailure(options, nameof(GeostationaryOptions.HazeAmount));
        }

        [TestCase(0.0f)]
        [TestCase(0.1f)]
        [TestCase(1.0f)]
        public void ValidHaze(float haze)
        {
            var options = ValidOptions();
            options.HazeAmount = haze;

            ValidateNoFailure(options, nameof(GeostationaryOptions.UnderlayPath));
        }

        [Test]
        public void MissingUnderlay()
        {
            var options = ValidOptions();
            options.UnderlayPath = $"{Guid.NewGuid()}";

            ValidateFailure(options, nameof(GeostationaryOptions.UnderlayPath));
        }

        [TestCase(0)]
        [TestCase(3)]
        public void InvalidSpatialResolution(int spatialResolution)
        {
            var options = ValidOptions();
            options.SpatialResolution = spatialResolution;

            ValidateFailure(options, nameof(GeostationaryOptions.SpatialResolution));
        }

        [TestCase(Constants.Satellite.SpatialResolution.TwoKm)]
        [TestCase(Constants.Satellite.SpatialResolution.FourKm)]
        public void ValidSpatialResolution(int spatialResolution)
        {
            var options = ValidOptions();
            options.SpatialResolution = spatialResolution;

            ValidateNoFailure(options, nameof(GeostationaryOptions.SpatialResolution));
        }

        [Test]
        public void OutputNotFileIfMultipleSource()
        {
            var options = ValidOptions();
            using var state = new FileState();
            var outputFile = state.CreateFile("foo.jpg");

            options.SourcePath = Path.Combine(state.CreateTempDirectory(), "*.jpg");
            options.OutputPath = outputFile;

            var results = new GeostationaryOptionsValidator().Validate(options);
            results.Errors.Select(e => e.ErrorMessage).Should().Contain("If multiple source files are specified, the output must be a directory.");
        }

        [Test]
        public void OutputDirectoryIfMultipleSources()
        {
            var options = ValidOptions();
            using var state = new FileState();

            options.SourcePath = Path.Combine(state.CreateTempDirectory(), "*.jpg");
            options.OutputPath = state.CreateTempDirectory();

            var results = new GeostationaryOptionsValidator().Validate(options);
            results.Errors.Select(e => e.ErrorMessage).Should().BeEmpty();
        }

        [Test]
        public void OutputNotDirectoryIfLongitudeSpecified()
        {
            var options = ValidOptions();
            options.Longitude = 147;
            
            using var state = new FileState();

            options.SourcePath = Path.Combine(state.CreateTempDirectory(), "*.jpg");
            options.OutputPath = state.CreateTempDirectory();

            var results = new GeostationaryOptionsValidator().Validate(options);
            results.Errors.Select(e => e.ErrorMessage).Should().Contain("If multiple source files are specified with a target latitude reprojection, the output cannot be a directory.");
        }

        [Test]
        public void OutputIsFileIfLongitudeSpecified()
        {
            var options = ValidOptions();
            options.Longitude = 147;
            
            using var state = new FileState();

            options.SourcePath = Path.Combine(state.CreateTempDirectory(), "*.jpg");
            options.OutputPath = Path.Combine(state.CreateTempDirectory(), "out.jpg");

            var results = new GeostationaryOptionsValidator().Validate(options);
            results.Errors.Select(e => e.ErrorMessage).Should().BeEmpty();
        }


        [TestCase(-181)]
        [TestCase(181)]
        public void InvalidLongitude(double longitude)
        {
            var options = ValidOptions();
            options.Longitude = longitude;

            ValidateFailure(options, nameof(GeostationaryOptions.Longitude));
        }

        [TestCase(-180)]
        [TestCase(180)]
        [TestCase(0.12345)]
        public void ValidLongitude(double longitude)
        {
            var options = ValidOptions();
            options.Longitude = longitude;

            ValidateNoFailure(options, nameof(GeostationaryOptions.Longitude));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("bananas")]
        public void InvalidTint(string tint)
        {
            var options = ValidOptions();
            options.Tint = tint;

            ValidateFailure(options, "Tint");
        }

        [TestCase("#ff0000")]
        [TestCase("ff0000")]
        [TestCase("00112233")]
        public void ValidTint(string tint)
        {
            var options = ValidOptions();
            options.Tint = tint;

            ValidateNoFailure(options, "Tint");
        }

        private GeostationaryOptions ValidOptions()
        {
            return new GeostationaryOptions
            {
                Tint = "0000FF",
                SpatialResolution = Constants.Satellite.SpatialResolution.TwoKm
            };
        }

        private static void ValidateFailure(GeostationaryOptions options, string propertyName)
        {
            var results = new GeostationaryOptionsValidator().Validate(options);
            results.Errors.Select(e => e.PropertyName).Should().Contain(propertyName);
        }

        private static void ValidateNoFailure(GeostationaryOptions options, string propertyName)
        {
            var results = new GeostationaryOptionsValidator().Validate(options);
            results.Errors.Select(e => e.PropertyName).Should().NotContain(propertyName);
        }
    }
}