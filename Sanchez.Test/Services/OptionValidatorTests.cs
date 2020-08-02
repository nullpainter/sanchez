using System.IO;
using FluentAssertions;
using NUnit.Framework;
using Sanchez.Models;
using Sanchez.Services;
using Sanchez.Test.Helper;

namespace Sanchez.Test.Services
{
    [TestFixture(TestOf = typeof(OptionValidator))]
    public class OptionValidatorTests : ServiceTests
    {
        private IOptionValidator OptionValidator => GetService<IOptionValidator>();

        [TestCase("pinky.jpg", ExpectedResult = true)]
        [TestCase("pinky.JPEG", ExpectedResult = true)]
        [TestCase("brain.png", ExpectedResult = true)]
        [TestCase("brain", ExpectedResult = false)]
        [TestCase("brain.bogus", ExpectedResult = false)]
        public bool VerifyImageFormat(string filename)
        {
            using var state = new FileState();

            // Prepare source files
            var imageFolder = state.CreateTempDirectory();
            var sourcePath = Path.Combine(imageFolder, "source.jpg");
            File.WriteAllText(sourcePath, "Hey!");

            var options = new CommandLineOptions
            {
                OutputPath = filename,
                SourcePath = sourcePath
            };

            // Run method under test
            return OptionValidator.Validate(options);
        }

        private static CommandLineOptions ValidOptions(FileState fileState)
        {
            var options = new CommandLineOptions
            {
                OutputPath = fileState.CreateFile("output.jpg"),
                UnderlayPath = fileState.CreateFile("underlay.jpg"),
                SourcePath = fileState.CreateFile("source.jpg"),
                OverlayPath = fileState.CreateFile("overlay.jpg"),
                Threads = 5
            };
            return options;
        }

        [Test]
        public void AllInvalidPaths()
        {
            var options = new CommandLineOptions
            {
                MaskPath = "Frank",
                UnderlayPath = "Jimmy",
                OverlayPath = "Ian",
                SourcePath = "Euclid"
            };

            // Run method under test
            var valid = OptionValidator.ValidatePaths(options, out var invalidPaths);

            valid.Should().BeFalse();
            invalidPaths.Should().BeEquivalentTo("Frank", "Jimmy", "Ian");

            // Run full validation
            valid = OptionValidator.Validate(options);
            valid.Should().BeFalse();
        }

        [Test]
        public void AllValid()
        {
            using var fileState = FileHelper.NewState();

            var options = ValidOptions(fileState);

            // Run method under test
            var valid = OptionValidator.ValidatePaths(options, out var invalidPaths);

            invalidPaths.Should().BeEmpty();
            valid.Should().BeTrue();
        }

        [Test]
        public void BatchSourceExists()
        {
            using var fileState = FileHelper.NewState();
            var temporaryDirectory = fileState.CreateTempDirectory();
            File.WriteAllText(Path.Combine(temporaryDirectory, "source"), "hey");

            var options = new CommandLineOptions
            {
                OutputPath = fileState.CreateFile("output.jpg"),
                UnderlayPath = fileState.CreateFile("underlay.jpg"),
                SourcePath = temporaryDirectory,
                OverlayPath = fileState.CreateFile("overlay.jpg"),
            };

            // Run method under test
            var valid = OptionValidator.Validate(options);
            valid.Should().BeTrue();
        }

        [Test]
        public void BatchSourceNotExists()
        {
            using var fileState = FileHelper.NewState();

            var options = new CommandLineOptions
            {
                OutputPath = fileState.CreateFile("output.jpg"),
                UnderlayPath = fileState.CreateFile("underlay.jpg"),
                SourcePath = "biscuits/**",
                OverlayPath = fileState.CreateFile("overlay.jpg"),
            };

            // Run method under test
            var valid = OptionValidator.Validate(options);
            valid.Should().BeTrue("batch validation not performed");
        }

        [Test]
        public void InvalidThreadCount()
        {
            using var fileState = FileHelper.NewState();

            var options = ValidOptions(fileState);
            options.Threads = 0;

            // Run method under test
            var valid = OptionValidator.ValidatePaths(options, out var invalidPaths);

            invalidPaths.Should().BeEmpty();
            valid.Should().BeFalse("invalid thread count should return an error");
        }

        [Test]
        public void PartialInvalidPaths()
        {
            var options = new CommandLineOptions
            {
                MaskPath = "Ian"
            };

            // Run method under test
            var valid = OptionValidator.ValidatePaths(options, out var invalidPaths);
            valid.Should().BeFalse();

            invalidPaths.Should().BeEquivalentTo("Ian");
        }
    }
}