using FluentAssertions;
using NUnit.Framework;
using Sanchez.Models;
using Sanchez.Services;
using Sanchez.Test.Helper;

namespace Sanchez.Test.Services
{
    [TestFixture(TestOf = typeof(CommandLineValidator))]
    public class CommandLineValidatorTests
    {
        [Test]
        public void PartialInvalidArguments()
        {
            var options = new CommandLineOptions
            {
                MaskPath = "Ian"
            };

            // Run method under test
            var valid = CommandLineValidator.VerifyPaths(options, out var invalidPaths);
            valid.Should().BeFalse();

            invalidPaths.Should().BeEquivalentTo("Ian");
        }

        [Test]
        public void AllInvalidArguments()
        {
            var options = new CommandLineOptions
            {
                MaskPath = "Frank",
                SourceImagePath = "Ruth",
                UnderlayPath = "Jimmy"
            };

            // Run method under test
            var valid = CommandLineValidator.VerifyPaths(options, out var invalidPaths);

            valid.Should().BeFalse();
            invalidPaths.Should().BeEquivalentTo("Frank", "Ruth", "Jimmy");
        }

        [Test]
        public void ValidArguments()
        {
            using var fileState = FileHelper.NewState();

            var options = new CommandLineOptions
            {
                OutputFile = fileState.CreateFile("output.jpg"),
                UnderlayPath = fileState.CreateFile("underlay.jpg"),
                SourceImagePath = fileState.CreateFile("source.jpg")
            };

            // Run method under test
            var valid = CommandLineValidator.VerifyPaths(options, out var invalidPaths);

            invalidPaths.Should().BeEmpty();
            valid.Should().BeTrue();
        }
    }
}