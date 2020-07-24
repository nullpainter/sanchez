using FluentAssertions;
using NUnit.Framework;
using Sanchez.Factories;
using Sanchez.Models;
using Sanchez.Services;
using Sanchez.Test.Helper;

namespace Sanchez.Test.Services
{
    [TestFixture(TestOf = typeof(CommandLineValidator))]
    public class CommandLineValidatorTests
    {
        [TestCase("pinky.jpg", ExpectedResult = true)]
        [TestCase("pinky.JPEG", ExpectedResult = true)]
        [TestCase("brain.png", ExpectedResult = true)]
        [TestCase("brain", ExpectedResult = false)]
        [TestCase("brain.bogus", ExpectedResult = false)]
        public bool VerifyImageFormat(string filename)
        {
            var options = new CommandLineOptions
            {
                OutputFile = filename
            };

            var renderOptions = RenderOptionFactory.ToRenderOptions(options);
            return CommandLineValidator.Validate(options, renderOptions);
        }

        [Test]
        public void AllInvalidPaths()
        {
            var options = new CommandLineOptions
            {
                MaskPath = "Frank",
                SourceImagePath = "Ruth",
                UnderlayPath = "Jimmy",
                OverlayPath = "Ian"
            };

            // Run method under test
            var valid = CommandLineValidator.ValidatePaths(options, out var invalidPaths);

            valid.Should().BeFalse();
            invalidPaths.Should().BeEquivalentTo("Frank", "Ruth", "Jimmy", "Ian");
        }

        [Test]
        public void PartialInvalidPaths()
        {
            var options = new CommandLineOptions
            {
                MaskPath = "Ian"
            };

            // Run method under test
            var valid = CommandLineValidator.ValidatePaths(options, out var invalidPaths);
            valid.Should().BeFalse();

            invalidPaths.Should().BeEquivalentTo("Ian");
        }

        [Test]
        public void ValidPaths()
        {
            using var fileState = FileHelper.NewState();

            var options = new CommandLineOptions
            {
                OutputFile = fileState.CreateFile("output.jpg"),
                UnderlayPath = fileState.CreateFile("underlay.jpg"),
                SourceImagePath = fileState.CreateFile("source.jpg"),
                OverlayPath = fileState.CreateFile("overlay.jpg"),
            };

            // Run method under test
            var valid = CommandLineValidator.ValidatePaths(options, out var invalidPaths);

            invalidPaths.Should().BeEmpty();
            valid.Should().BeTrue();
        }
    }
}