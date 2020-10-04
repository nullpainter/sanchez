using System.IO;
using FluentAssertions;
using NUnit.Framework;
using Sanchez.Processing.Filesystem;
using Sanchez.Test.Common;

namespace Sanchez.Processing.Test.Filesystem
{
    [TestFixture(TestOf = typeof(SingleFilenameProvider))]
    public class SingleFilenameProviderTests : AbstractTests
    {
        private SingleFilenameProvider FilenameProvider => GetService<SingleFilenameProvider>();
        
        [Test]
        public void OutputFilenameNonBatch()
        {
            RenderOptions.SourcePath = "source.jpg";
            RenderOptions.OutputPath = Path.Combine("test", "output.jpg");

            var outputFilename = FilenameProvider.GetOutputFilename("source.jpg");
            outputFilename.Should().Be(Path.Combine("test", "output.jpg"));
        }
        
        [Test]
        public void OutputFilenameBatch()
        {
            RenderOptions.SourcePath = "images/**";
            RenderOptions.OutputPath = Path.Combine("test", "output");

            var outputFilename = FilenameProvider.GetOutputFilename("source.jpg");
            outputFilename.Should().Be(Path.Combine("test", "output", "source-FC.jpg"));
        }
    }
}