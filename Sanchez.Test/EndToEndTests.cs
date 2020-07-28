using System.IO;
using FluentAssertions;
using NUnit.Framework;
using Sanchez.Test.Helper;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Sanchez.Test
{
    [TestFixture(TestOf = typeof(Sanchez))]
    public class EndToEndTests : ServiceTests
    {
        private static void CreateImage(string tempDirectory, string filename)
        {
            var path = Path.Combine(tempDirectory, filename);

            using var image = new Image<Rgba32>(2000, 2000);
            using var stream = new FileStream(path, FileMode.Create);

            image.SaveAsJpeg(stream);
        }

        [Test]
        public void ValidInputFilesRequired()
        {
            using var fileState = FileHelper.NewState();
            var tempDirectory = fileState.CreateTempDirectory();

            const string underlayFilename = "underlay.jpg";
            const string outputFilename = "output.jpg";

            CreateImage(tempDirectory, underlayFilename);

            var outputPath = Path.Combine(tempDirectory, outputFilename);

            // Run method under test
            Sanchez.Main(
                "-s", "biscuits",
                "-u", Path.Combine(tempDirectory, underlayFilename),
                "-o", outputPath
            );

            File.Exists(outputPath).Should().BeFalse("output file should not have been created if input files were missing");
        }

        [Test]
        public void ValidOutputFormatRequired()
        {
            using var fileState = FileHelper.NewState();
            var tempDirectory = fileState.CreateTempDirectory();

            const string underlayFilename = "underlay.jpg";
            const string satelliteFilename = "satellite.jpg";
            const string outputFilename = "output.bogus";

            CreateImage(tempDirectory, underlayFilename);
            CreateImage(tempDirectory, satelliteFilename);

            var outputPath = Path.Combine(tempDirectory, outputFilename);

            // Run method under test
            Sanchez.Main(
                "-s", Path.Combine(tempDirectory, satelliteFilename),
                "-u", Path.Combine(tempDirectory, underlayFilename),
                "-o", outputPath
            );

            File.Exists(outputPath).Should().BeFalse("no output file should be written if the output format can't be determined");
        }

        [Test]
        public void WithMaskAndOverlay()
        {
            using var fileState = FileHelper.NewState();
            var tempDirectory = fileState.CreateTempDirectory();

            const string maskFilename = "mask.jpg";
            const string overlayFilename = "overlay.jpg";
            const string underlayFilename = "underlay.jpg";
            const string satelliteFilename = "satellite.jpg";
            const string outputFilename = "output.jpg";

            CreateImage(tempDirectory, maskFilename);
            CreateImage(tempDirectory, overlayFilename);
            CreateImage(tempDirectory, underlayFilename);
            CreateImage(tempDirectory, satelliteFilename);

            var outputPath = Path.Combine(tempDirectory, outputFilename);

            // Run method under test
            Sanchez.Main(
                "-s", Path.Combine(tempDirectory, satelliteFilename),
                "-m", Path.Combine(tempDirectory, maskFilename),
                "-u", Path.Combine(tempDirectory, underlayFilename),
                "-O", Path.Combine(tempDirectory, overlayFilename),
                "-o", outputPath,
                "-t", "00BBFF"
            );

            File.Exists(outputPath).Should().BeTrue("output file should have been created");
            using var outputImage = Image.Load(outputPath);
            outputImage.Width.Should().Be(2000);
            outputImage.Height.Should().Be(2000);
        }

        [Test]
        public void ExistingFileNotOverwritten()
        {
            using var fileState = FileHelper.NewState();
            var tempDirectory = fileState.CreateTempDirectory();

            const string underlayFilename = "underlay.jpg";
            const string satelliteFilename = "satellite.jpg";
            const string outputFilename = "output.jpg";

            CreateImage(tempDirectory, underlayFilename);
            CreateImage(tempDirectory, satelliteFilename);

            // Create output file
            File.WriteAllText(Path.Combine(tempDirectory, outputFilename), "Don't hurt me!");

            var outputPath = Path.Combine(tempDirectory, outputFilename);

            // Run method under test
            Sanchez.Main(
                "-s", Path.Combine(tempDirectory, satelliteFilename),
                "-u", Path.Combine(tempDirectory, underlayFilename),
                "-o", outputPath
            );

            File.ReadAllText(outputPath).Should().Be("Don't hurt me!", "existing file shouldn't have been overwritten");
        }

        [Test]
        public void ExistingBatchFileNotOverridden()
        {
            using var fileState = FileHelper.NewState();
            var tempDirectory = fileState.CreateTempDirectory();

            const string underlayFilename = "underlay.jpg";
            const string satelliteFilename = "satellite.jpg";
            const string outputFilename = "satellite-fc.jpg";

            CreateImage(tempDirectory, underlayFilename);
            CreateImage(tempDirectory, satelliteFilename);

            // Create output file
            var outputDirectory = Path.Combine(tempDirectory, "output");
            Directory.CreateDirectory(outputDirectory);

            var outputImagePath = Path.Combine(outputDirectory, outputFilename);
            File.WriteAllText(outputImagePath, "Don't hurt me!");

            // Run method under test
            Sanchez.Main(
                "-q",
                "-s", Path.Combine(tempDirectory, "*.*"),
                "-u", Path.Combine(tempDirectory, underlayFilename),
                "-o", outputDirectory
            );

            File.ReadAllText(outputImagePath).Should().Be("Don't hurt me!", "existing file shouldn't have been overwritten");
        }

        [Test]
        public void WithoutMask()
        {
            using var fileState = FileHelper.NewState();
            var tempDirectory = fileState.CreateTempDirectory();

            const string underlayFilename = "underlay.jpg";
            const string satelliteFilename = "satellite.jpg";
            const string outputFilename = "output.jpg";

            CreateImage(tempDirectory, underlayFilename);
            CreateImage(tempDirectory, satelliteFilename);

            var outputPath = Path.Combine(tempDirectory, outputFilename);

            // Run method under test
            Sanchez.Main(
                "-s", Path.Combine(tempDirectory, satelliteFilename),
                "-u", Path.Combine(tempDirectory, underlayFilename),
                "-o", outputPath,
                "-b", "1.0",
                "-S", "0.4",
                "-t", "#ff00ff"
            );

            File.Exists(outputPath).Should().BeTrue("output file should have been created");
            using var outputImage = Image.Load(outputPath);
            outputImage.Width.Should().Be(2000);
            outputImage.Height.Should().Be(2000);
        }
        
        [Test]
        public void InvalidTint()
        {
            using var fileState = FileHelper.NewState();
            var tempDirectory = fileState.CreateTempDirectory();

            const string underlayFilename = "underlay.jpg";
            const string satelliteFilename = "satellite.jpg";
            const string outputFilename = "output.jpg";

            CreateImage(tempDirectory, underlayFilename);
            CreateImage(tempDirectory, satelliteFilename);

            var outputPath = Path.Combine(tempDirectory, outputFilename);

            // Run method under test
            Sanchez.Main(
                "-s", Path.Combine(tempDirectory, satelliteFilename),
                "-u", Path.Combine(tempDirectory, underlayFilename),
                "-o", outputPath,
                "-b", "1.0",
                "-S", "0.4",
                "-t", "bananas"
            );

            File.Exists(outputPath).Should().BeFalse("output file should not have been created with an invalid tint");
        }
        
        [Test]
        public void NoSatelliteFiles()
        {
            using var fileState = FileHelper.NewState();
            var tempDirectory = fileState.CreateTempDirectory();

            const string underlayFilename = "underlay.jpg";
            var satellitePath = Path.Combine(tempDirectory, "satellite");
            const string outputFilename = "output.jpg";

            CreateImage(tempDirectory, underlayFilename);

            Directory.CreateDirectory(satellitePath);

            var outputPath = Path.Combine(tempDirectory, outputFilename);

            // Run method under test
            Sanchez.Main(
                "-s", Path.Combine(tempDirectory, satellitePath),
                "-u", Path.Combine(tempDirectory, underlayFilename),
                "-o", outputPath,
                "-b", "1.0",
                "-S", "0.4"
            );

            File.Exists(outputPath).Should().BeFalse("output file should not have been created if there are no source files");
        }
    }
}