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
    [TestFixture(TestOf = typeof(EquirectangularRenderOptions))]
    public class EquirectangularOptionsTests
    {
        [Test]
        public void ValidOutput()
        {
            var options = ValidOptions();
            using var state = new FileState();
            var outputFile = state.CreateFile("foo.jpg");

            options.SourcePath = Path.Combine(state.CreateTempDirectory(), "*.jpg");
            options.OutputPath = outputFile;

            var results = new EquirectangularOptionsValidator().Validate(options);
            results.Errors.Select(e => e.ErrorMessage).Should().BeEmpty();
        }

        [Test]
        public void OutputIsDirectory()
        {
            var options = ValidOptions();
            using var state = new FileState();

            options.SourcePath = Path.Combine(state.CreateTempDirectory(), "*.jpg");
            options.OutputPath = state.CreateTempDirectory();

            var results = new EquirectangularOptionsValidator().Validate(options);
            results.Errors.Select(e => e.ErrorMessage).Should().Contain("The output cannot be a directory if combining satellite imagery.");
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