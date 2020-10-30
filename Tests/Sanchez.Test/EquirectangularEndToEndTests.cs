using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Sanchez.Processing.Test.Helper;
using SixLabors.ImageSharp;

namespace Sanchez.Test
{
    [TestFixture(TestOf = typeof(Bootstrapper))]
    public class EquirectangularEndToEndTests : EndToEndTestTests
    {
        [Test]
        public async Task MissingSource()
        {
            var outputDirectory = State.CreateTempDirectory();
            var outputFile = Path.Combine(outputDirectory, "out.jpg");

            var returnCode = await Bootstrapper.Main(
                "reproject",
                "-vo", outputFile);

            VerifyFailedExecution(returnCode);
        }

        [Test]
        public async Task Single()
        {
            const string sourceFile = "GOES17_FD_CH13_20200830T033031Z.jpg";

            var rootDirectory = await CreateSingleSimpleImageAsync(State, sourceFile);

            var outputDirectory = State.CreateTempDirectory();
            var outputFile = Path.Combine(outputDirectory, "out.jpg");

            var returnCode = await Bootstrapper.Main(
                "reproject",
                "-s", Path.Combine(rootDirectory, sourceFile),
                "-c", "0.0-0.2",
                "-vo", outputFile);

            VerifySuccessfulExecution(returnCode);

            File.Exists(outputFile).Should().BeTrue("output file should have been created");
            var outputImage = await Image.LoadAsync(outputFile);
            outputImage.Width.Should().Be(2449);
            outputImage.Height.Should().Be(2450);
        }

        [Test]
        public async Task Multiple()
        {
            var rootDirectory = await CreateSampleImagesAsync(State);
            var outputDirectory = State.CreateTempDirectory();

            var returnCode = await Bootstrapper.Main(
                "reproject",
                "-s", rootDirectory,
                "-vo", outputDirectory);

            VerifySuccessfulExecution(returnCode);

            Directory.Exists(outputDirectory).Should().BeTrue("output directory should have been created");
            Directory.GetFiles(outputDirectory).Should().HaveCount(8);

            foreach (var outputFile in Directory.GetFiles(outputDirectory))
            {
                var outputImage = await Image.LoadAsync(outputFile);
                outputImage.Width.Should().BeGreaterThan(0);
                outputImage.Height.Should().BeGreaterThan(0);
            }
        }

        [Test]
        public async Task MultipleStitch()
        {
            var rootDirectory = await CreateSampleImagesAsync(State);
            var outputDirectory = State.CreateTempDirectory();
            var outputFile = Path.Combine(outputDirectory, "out.jpg");

            var returnCode = await Bootstrapper.Main(
                "reproject",
                "-s", rootDirectory,
                "-o", outputFile,
                "-vT", "2020-08-30T03:50:20");

            VerifySuccessfulExecution(returnCode);

            File.Exists(outputFile).Should().BeTrue("output file should have been created");
            var outputImage = await Image.LoadAsync(outputFile);

            outputImage.Width.Should().Be(4794);
            outputImage.Height.Should().Be(2450);
        }

        [Test]
        public async Task StitchWithCrop()
        {
            var rootDirectory = await CreateSampleImagesAsync(State);
            var outputDirectory = State.CreateTempDirectory();
            var outputFile = Path.Combine(outputDirectory, "out.jpg");

            var returnCode = await Bootstrapper.Main(
                "reproject",
                "-s", rootDirectory,
                "-o", outputFile,
                "-a",
                "-c", "0.0-1.0",
                "-T", "2020-08-30T03:50:20");

            VerifySuccessfulExecution(returnCode);

            File.Exists(outputFile).Should().BeTrue("output file should have been created");
            var outputImage = await Image.LoadAsync(outputFile);

            outputImage.Width.Should().Be(4066);
            outputImage.Height.Should().Be(2076);
        }

        [Test]
        public async Task TimestampStitchNoStartTime()
        {
            var rootDirectory = await CreateSampleImagesAsync(State);
            var outputDirectory = State.CreateTempDirectory();

            var returnCode = await Bootstrapper.Main(
                "reproject",
                "-s", rootDirectory,
                "-o", outputDirectory,
                "-I", "60",
                "-v");

            VerifySuccessfulExecution(returnCode);

            Directory.Exists(outputDirectory).Should().BeTrue("output directory should have been created");
            var outputFiles = Directory.GetFiles(outputDirectory);
            outputFiles.Length.Should().Be(2);
        }

        [Test]
        public async Task TimestampStitch()
        {
            var rootDirectory = await CreateSampleImagesAsync(State);
            var outputDirectory = State.CreateTempDirectory();

            var returnCode = await Bootstrapper.Main(
                "reproject",
                "-s", rootDirectory,
                "-o", outputDirectory,
                "-a",
                "-I", "60",
                "-d", "60",
                "-v",
                "-T", "2020-08-30T03:00:20",
                "-e", "2020-08-31T03:00:20"
            );

            VerifySuccessfulExecution(returnCode);

            Directory.Exists(outputDirectory).Should().BeTrue("output directory should have been created");
            var outputFiles = Directory.GetFiles(outputDirectory);
            outputFiles.Length.Should().Be(3);
        }

        [Test]
        public async Task TimestampStitchLargeInterval()
        {
            var rootDirectory = await CreateSampleImagesAsync(State);
            var outputDirectory = State.CreateTempDirectory();

            var returnCode = await Bootstrapper.Main(
                "reproject",
                "-s", rootDirectory,
                "-o", outputDirectory,
                "-a",
                "-I", "500",
                "-d", "60",
                "-T", "2020-08-30T03:00:20");

            VerifySuccessfulExecution(returnCode);

            Directory.Exists(outputDirectory).Should().BeTrue("output directory should have been created");
            var outputFiles = Directory.GetFiles(outputDirectory);
            outputFiles.Length.Should().Be(1);
        }

        [Test]
        public async Task SingleWithCrop()
        {
            const string sourceFile = "GOES17_FD_CH13_20200830T033031Z.jpg";

            var rootDirectory = await CreateSingleSimpleImageAsync(State, sourceFile);

            var outputDirectory = State.CreateTempDirectory();
            var outputFile = Path.Combine(outputDirectory, "out.jpg");

            var returnCode = await Bootstrapper.Main(
                "reproject",
                "-s", Path.Combine(rootDirectory, sourceFile),
                "-o", outputFile,
                "-av");

            VerifySuccessfulExecution(returnCode);

            File.Exists(outputFile).Should().BeTrue("output file should have been created");
            var outputImage = await Image.LoadAsync(outputFile);
            outputImage.Width.Should().Be(1918);
            outputImage.Height.Should().Be(1916);
        }

        
        [Test]
        public async Task SingleWithCropAndOverlay()
        {
            const string sourceFile = "GOES17_FD_CH13_20200830T033031Z.jpg";

            var rootDirectory = await CreateSingleSimpleImageAsync(State, sourceFile);
            await CreateImage(Path.Combine(rootDirectory, "Overlay.jpg"));
            
            var outputDirectory = State.CreateTempDirectory();
            var outputFile = Path.Combine(outputDirectory, "out.jpg");

            var returnCode = await Bootstrapper.Main(
                "reproject",
                "-s", Path.Combine(rootDirectory, sourceFile),
                "-o", outputFile,
                "-c", "0-0.9",
                "-av");

            VerifySuccessfulExecution(returnCode);

            File.Exists(outputFile).Should().BeTrue("output file should have been created");
            var outputImage = await Image.LoadAsync(outputFile);
            outputImage.Width.Should().Be(1918);
            outputImage.Height.Should().Be(1916);
        }
        
        [Test]
        public async Task Quiet()
        {
            const string sourceFile = "GOES17_FD_CH13_20200830T033031Z.jpg";

            var rootDirectory = await CreateSingleSimpleImageAsync(State, sourceFile);

            var outputDirectory = State.CreateTempDirectory();
            var outputFile = Path.Combine(outputDirectory, "out.jpg");

            var writer = new StringWriter();
            Console.SetOut(writer);

            var returnCode = await Bootstrapper.Main(
                "reproject",
                "-s", Path.Combine(rootDirectory, sourceFile),
                "-o", outputFile,
                "-aq");

            VerifySuccessfulExecution(returnCode);

            File.Exists(outputFile).Should().BeTrue("output file should have been created");
            var outputImage = await Image.LoadAsync(outputFile);
            outputImage.Width.Should().Be(1918);
            outputImage.Height.Should().Be(1916);

            writer.ToString().Should().BeEmpty("no output should be written in quiet mode");
        }

        private async Task<string> CreateSingleSimpleImageAsync(FileState state, string filename)
        {
            var rootDirectory = state.CreateTempDirectory();

            // Create sample files
            await CreateImage(Path.Combine(rootDirectory, filename));
            return rootDirectory;
        }
    }
}