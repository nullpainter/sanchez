using System.IO;
using System.Linq;
using Extend;
using FluentAssertions;
using Funhouse.Services;
using Funhouse.Test.Helper;
using NUnit.Framework;

namespace Funhouse.Test.Filesystem
{
    [TestFixture(TestOf = typeof(FileService))]
    public class FileServiceTests : AbstractTests
    {
        private IFileService FileService => GetService<IFileService>();

        [Test(Description = "Verifies that the output folder is created for batches if it doesn't already exist")]
        public void OutputFolderCreatedIfBatch()
        {
            using var state = new FileState();
            var imageFolder = state.CreateTempDirectory();
            var outputPath = Path.Combine(imageFolder, "output");

            RenderOptions.SourcePath = Path.Combine(imageFolder, "images/*.*");
            RenderOptions.OutputPath = outputPath;

            // Sanity check
            RenderOptions.MultipleSources.Should().BeTrue();
            Directory.Exists(outputPath).Should().BeFalse();

            // Run method under test
            FileService.PrepareOutput();
            Directory.Exists(outputPath).Should().BeTrue("output directory should be created for batch processing");
        }

        [Test(Description = "Verifies that the output folder isn't created if it already exists")]
        public void OutputFolderNotCreatedIfPresent()
        {
            using var state = new FileState();
            var imageFolder = state.CreateTempDirectory();
            var outputPath = Path.Combine(imageFolder, "output");
            Directory.CreateDirectory(outputPath);

            RenderOptions.SourcePath = Path.Combine(imageFolder, "images/*.*");
            RenderOptions.OutputPath = outputPath;

            // Sanity check
            RenderOptions.MultipleSources.Should().BeTrue();
            Directory.Exists(outputPath).Should().BeTrue();

            // Run method under test
            FileService.PrepareOutput();
            Directory.Exists(outputPath).Should().BeTrue();
        }

        [Test(Description = "Verifies that the output is not treated as a folder when processing a single input file")]
        public void OutputFolderNotCreatedForSingle()
        {
            using var state = new FileState();
            var imageFolder = state.CreateTempDirectory();
            var outputPath = Path.Combine(imageFolder, "output", "output.jpg");

            RenderOptions.SourcePath = Path.Combine(imageFolder, "images/source.jpg");
            RenderOptions.OutputPath = outputPath;

            // Sanity check
            RenderOptions.MultipleTargets.Should().BeFalse();

            // Run method under test
            FileService.PrepareOutput();
            Directory.Exists(outputPath).Should().BeFalse("no directory should be created for non-batch mode");
            File.Exists(outputPath).Should().BeFalse();
        }

        [Test]
        public void OutputFilenameBatch()
        {
            RenderOptions.SourcePath = "images/**";
            RenderOptions.OutputPath = Path.Combine("test", "output");

            var outputFilename = FileService.GetOutputFilename("source.jpg");
            outputFilename.Should().Be(Path.Combine("test", "output", "source-fc.jpg"));
        }

        [Test]
        public void OutputFilenameNonBatch()
        {
            RenderOptions.SourcePath = "source.jpg";
            RenderOptions.OutputPath = Path.Combine("test", "output.jpg");

            var outputFilename = FileService.GetOutputFilename("source.jpg");
            outputFilename.Should().Be(Path.Combine("test", "output.jpg"));
        }

        [Test]
        public void GetSourceFilesSingle()
        {
            using var state = new FileState();

            var imageFolder = state.CreateTempDirectory();
            var sourceFile = Path.Combine(imageFolder, "source.jpg");
            var outputPath = Path.Combine(imageFolder, "output", "output.jpg");

            File.WriteAllText(sourceFile, "Hey!");

            RenderOptions.SourcePath = sourceFile;
            RenderOptions.OutputPath = outputPath;

            // Run method under test
            FileService.GetSourceFiles().Should().BeEquivalentTo(sourceFile);
        }

#if OS_WINDOWS
        private const string FirstPath = "source\\first\\2020\\firstImage.jpg";
        private const string SecondPath = "source\\first\\2020\\secondImage.png";
        private const string ThirdPath = "source\\second\\thirdImage.png";
#else
        private const string FirstPath = "source/first/2020/firstImage.jpg";
        private const string SecondPath = "source/first/2020/secondImage.png";
        private const string ThirdPath = "source/second/thirdImage.png";
#endif

        [TestCase("**/thirdImage.pn?", ExpectedResult = new[] { ThirdPath })]
        [TestCase("source/", ExpectedResult = new[] { FirstPath, SecondPath, ThirdPath })]
        [TestCase("source", ExpectedResult = new[] { FirstPath, SecondPath, ThirdPath })]
        [TestCase("source/**/*.*", ExpectedResult = new[] { FirstPath, SecondPath, ThirdPath })]
        [TestCase("source/f*/**", ExpectedResult = new[] { FirstPath, SecondPath })]
        [TestCase("source/**/*.png", ExpectedResult = new[] { SecondPath, ThirdPath })]
        [TestCase("source/first/2020/*.*", ExpectedResult = new[] { FirstPath, SecondPath })]
        [TestCase("source/**/2*/*.*", ExpectedResult = new[] { FirstPath, SecondPath })]
        public string[] GetSourceFilesBatch(string sourcePath)
        {
            using var state = new FileState();

            // Prepare source files
            var imageFolder = state.CreateTempDirectory();
            var outputPath = Path.Combine(imageFolder, "output", "output.jpg");

            Directory.CreateDirectory(Path.Combine(imageFolder, "source", "first", "2020"));
            Directory.CreateDirectory(Path.Combine(imageFolder, "source", "second"));

            var files = new[]
            {
                Path.Combine(imageFolder, "source", "first", "2020", "firstImage.jpg"),
                Path.Combine(imageFolder, "source", "first", "2020", "secondImage.png"),
                Path.Combine(imageFolder, "source", "second", "thirdImage.png")
            };

            files.ForEach(path => File.WriteAllText(path, "Isn't this fun?"));

            RenderOptions.SourcePath = Path.Combine(imageFolder, sourcePath);
            RenderOptions.OutputPath = outputPath;

            // Run method under test
            return FileService
                .GetSourceFiles()
                .OrderBy(f => f)
                .Select(file => Path.GetRelativePath(imageFolder, file))
                .ToArray();
        }
    }
}