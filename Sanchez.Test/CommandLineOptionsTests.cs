using System.IO;
using FluentAssertions;
using NUnit.Framework;
using Sanchez.Models;
using Sanchez.Test.Helper;

namespace Sanchez.Test
{
    [TestFixture]
    public class CommandLineOptionsTests
    {
        [TestCase("foo.jpg", ExpectedResult = false)]
        [TestCase("images/*.*", ExpectedResult = true)]
        [TestCase("images/**/*FC*.jpg", ExpectedResult = true)]
        public bool VerifyBatchFlagGlob(string path)
        {
            var options = new CommandLineOptions
            {
                SourcePath = path
            };

            return options.IsBatch;
        }

        [Test]
        public void VerifyBatchFlagDirectory()
        {
            using var state = new FileState();
            var directory = state.CreateTempDirectory();

            var options = new CommandLineOptions
            {
                SourcePath = directory
            };

            options.IsBatch.Should().BeTrue("batch mode should be enabled if the source is a directory");
        }

        [Test]
        public void VerifyNoBatchFlagFile()
        {
            using var state = new FileState();
            var directory = state.CreateTempDirectory();
            var outputPath = Path.Combine(directory, "source.jpg");
            File.WriteAllText(outputPath, "Hello!");

            // Sanity check
            File.Exists(outputPath).Should().BeTrue();

            var options = new CommandLineOptions
            {
                SourcePath = outputPath
            };

            options.IsBatch.Should().BeFalse("batch mode should not be enabled for a single file");
        }
    }
}