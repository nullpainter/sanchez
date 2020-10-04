using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Sanchez.Processing.Models;
using Sanchez.Processing.Test.Helper;
using Sanchez.Test.Common;
using SixLabors.ImageSharp;

namespace Sanchez.Test
{
    [TestFixture(TestOf = typeof(Bootstrapper))]
    public class EndToEndTests : AbstractTests
    {
        [Test]
        public async Task MissingSource()
        {
            using var fileState = new FileState();

            var outputDirectory = fileState.CreateTempDirectory();
            var outputFile = Path.Combine(outputDirectory, "out.jpg");

            var returnCode = await Bootstrapper.Main(
                "reproject",
                "-vo", outputFile);

            VerifyFailedExecution(returnCode);
        }

        [Test]
        public async Task EquirectangularSingle()
        {
            using var fileState = new FileState();
            const string sourceFile = "GOES17_FD_CH13_20200830T033031Z.jpg";

            var rootDirectory = await CreateSingleSimpleImageAsync(fileState, sourceFile);

            var outputDirectory = fileState.CreateTempDirectory();
            var outputFile = Path.Combine(outputDirectory, "out.jpg");

            var returnCode = await Bootstrapper.Main(
                "reproject",
                "-s", Path.Combine(rootDirectory, sourceFile),
                "-vo", outputFile);

            VerifySuccessfulExecution(returnCode);

            File.Exists(outputFile).Should().BeTrue("output file should have been created");
            var outputImage = await Image.LoadAsync(outputFile);
            outputImage.Width.Should().Be(2449);
            outputImage.Height.Should().Be(2450);
        }

        [Test]
        public async Task EquirectangularMultiple()
        {
            using var fileState = new FileState();
            var rootDirectory = await CreateSampleImagesAsync(fileState);
            var outputDirectory = fileState.CreateTempDirectory();

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
        public async Task EquirectangularMultipleStitch()
        {
            using var fileState = new FileState();
            var rootDirectory = await CreateSampleImagesAsync(fileState);
            var outputDirectory = fileState.CreateTempDirectory();
            var outputFile = Path.Combine(outputDirectory, "out.jpg");

            var returnCode = await Bootstrapper.Main(
                "reproject",
                "-s", rootDirectory,
                "-o", outputFile,
                "-T", "2020-08-30T03:50:20");

            VerifySuccessfulExecution(returnCode);

            File.Exists(outputFile).Should().BeTrue("output file should have been created");
            var outputImage = await Image.LoadAsync(outputFile);

            outputImage.Width.Should().Be(4794);
            outputImage.Height.Should().Be(2450);
        }

        [Test]
        public async Task EquirectangularStitchWithCrop()
        {
            using var fileState = new FileState();
            var rootDirectory = await CreateSampleImagesAsync(fileState);
            var outputDirectory = fileState.CreateTempDirectory();
            var outputFile = Path.Combine(outputDirectory, "out.jpg");

            var returnCode = await Bootstrapper.Main(
                "reproject",
                "-s", rootDirectory,
                "-o", outputFile,
                "-a",
                "-T", "2020-08-30T03:50:20");

            VerifySuccessfulExecution(returnCode);

            File.Exists(outputFile).Should().BeTrue("output file should have been created");
            var outputImage = await Image.LoadAsync(outputFile);

            outputImage.Width.Should().Be(4066);
            outputImage.Height.Should().Be(2076);
        }

        [Test]
        public async Task EquirectangularTimestampStitchNoStartTime()
        {
            using var fileState = new FileState();
            var rootDirectory = await CreateSampleImagesAsync(fileState);
            var outputDirectory = fileState.CreateTempDirectory();

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
        public async Task EquirectangularTimestampStitch()
        {
            using var fileState = new FileState();
            var rootDirectory = await CreateSampleImagesAsync(fileState);
            var outputDirectory = fileState.CreateTempDirectory();

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
        public async Task EquirectangularTimestampStitchLargeInterval()
        {
            using var fileState = new FileState();
            var rootDirectory = await CreateSampleImagesAsync(fileState);
            var outputDirectory = fileState.CreateTempDirectory();

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
        public async Task EquirectangularSingleWithCrop()
        {
            using var fileState = new FileState();
            const string sourceFile = "GOES17_FD_CH13_20200830T033031Z.jpg";

            var rootDirectory = await CreateSingleSimpleImageAsync(fileState, sourceFile);

            var outputDirectory = fileState.CreateTempDirectory();
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
        public async Task Quiet()
        {
            using var fileState = new FileState();
            const string sourceFile = "GOES17_FD_CH13_20200830T033031Z.jpg";

            var rootDirectory = await CreateSingleSimpleImageAsync(fileState, sourceFile);

            var outputDirectory = fileState.CreateTempDirectory();
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

        [Test]
        public async Task GeostationarySingle()
        {
            using var fileState = new FileState();
            var rootDirectory = await CreateSampleImagesAsync(fileState);
            var outputDirectory = fileState.CreateTempDirectory();
            var outputFile = Path.Combine(outputDirectory, "out.jpg");

            var returnCode = await Bootstrapper.Main(
                "-s", Path.Combine(rootDirectory, "GOES16_FD_CH13_20200830T035020Z.jpg"),
                "-o", outputFile,
                "-q");

            VerifySuccessfulExecution(returnCode);

            File.Exists(outputFile).Should().BeTrue("output file should have been created");
            var outputImage = await Image.LoadAsync(outputFile);
            outputImage.Width.Should().Be(Constants.Satellite.ImageSize.FourKm);
            outputImage.Height.Should().Be(Constants.Satellite.ImageSize.FourKm);
        }

        [Test]
        public async Task GeostationaryMultiple()
        {
            using var fileState = new FileState();
            var rootDirectory = await CreateSampleImagesAsync(fileState);
            var outputDirectory = fileState.CreateTempDirectory();

            var returnCode = await Bootstrapper.Main(
                "-s", rootDirectory,
                "-o", outputDirectory,
                "-q");

            VerifySuccessfulExecution(returnCode);

            Directory.Exists(outputDirectory).Should().BeTrue("output directory should have been created");
            Directory.GetFiles(outputDirectory).Should().HaveCount(8);

            foreach (var outputFile in Directory.GetFiles(outputDirectory))
            {
                var outputImage = await Image.LoadAsync(outputFile);
                outputImage.Width.Should().Be(Constants.Satellite.ImageSize.FourKm);
                outputImage.Height.Should().Be(Constants.Satellite.ImageSize.FourKm);
            }
        }

        [Test]
        public async Task GeostationaryReprojected()
        {
            using var fileState = new FileState();
            var rootDirectory = await CreateSampleImagesAsync(fileState);
            var outputDirectory = fileState.CreateTempDirectory();
            var outputFile = Path.Combine(outputDirectory, "out.jpg");

            var returnCode = await Bootstrapper.Main(
                "-s", rootDirectory,
                "-o", outputFile,
                "-l", "174",
                "-T 2020-08-30T03:50:20",
                "-q");

            VerifySuccessfulExecution(returnCode);

            File.Exists(outputFile).Should().BeTrue("output file should have been created");
            var outputImage = await Image.LoadAsync(outputFile);
            outputImage.Width.Should().Be(Constants.Satellite.ImageSize.FourKm);
            outputImage.Height.Should().Be(Constants.Satellite.ImageSize.FourKm);
        }

        [Test]
        public async Task GeostationaryReprojectedMinSatellites()
        {
            using var fileState = new FileState();
            var rootDirectory = await CreateSampleImagesAsync(fileState);
            var outputDirectory = fileState.CreateTempDirectory();
            var outputFile = Path.Combine(outputDirectory, "out.jpg");

            var returnCode = await Bootstrapper.Main(
                "-s", rootDirectory,
                "-o", outputFile,
                "-l", "174",
                "-m", "3",
                "-T 2020-08-30T03:50:20",
                "-q");

            VerifySuccessfulExecution(returnCode);

            File.Exists(outputFile).Should().BeTrue("output file should have been created");
            var outputImage = await Image.LoadAsync(outputFile);
            outputImage.Width.Should().Be(Constants.Satellite.ImageSize.FourKm);
            outputImage.Height.Should().Be(Constants.Satellite.ImageSize.FourKm);
        }

        [Test]
        public async Task GeostationaryReprojectedTooFewSatellites()
        {
            using var fileState = new FileState();
            var rootDirectory = await CreateSampleImagesAsync(fileState);
            var outputDirectory = fileState.CreateTempDirectory();
            var outputFile = Path.Combine(outputDirectory, "out.jpg");

            var returnCode = await Bootstrapper.Main(
                "-s", rootDirectory,
                "-o", outputFile,
                "-l", "174",
                "-m", "10",
                "-T 2020-08-30T03:50:20",
                "-q");

            VerifySuccessfulExecution(returnCode);

            File.Exists(outputFile).Should().BeFalse("fewer than 10 satellites are present");
        }

        // Not yet built
        // [Test]
        // public async Task GeostationaryTimestampReprojected()
        // {
        //     using var fileState = new FileState();
        //     var rootDirectory = await CreateSampleImagesAsync(fileState);
        //     var outputDirectory = fileState.CreateTempDirectory();
        //
        //     await Bootstrapper.Main(
        //         "-s", rootDirectory,
        //         "-o", outputDirectory,
        //         "-l", "174",
        //         "-I", "60",
        //         "-q");
        //
        //     Directory.Exists(outputDirectory).Should().BeTrue("output directory should have been created");
        //     Directory.GetFiles(outputDirectory).Should().HaveCount(8);
        //
        //     foreach (var outputFile in Directory.GetFiles(outputDirectory))
        //     {
        //         var outputImage = await Image.LoadAsync(outputFile);
        //         outputImage.Width.Should().Be(Constants.Satellite.ImageSize.FourKm);
        //         outputImage.Height.Should().Be(Constants.Satellite.ImageSize.FourKm);
        //     }
        // }

        private async Task<string> CreateSingleSimpleImageAsync(FileState state, string filename)
        {
            var rootDirectory = state.CreateTempDirectory();

            // Create sample files
            await CreateImage(Path.Combine(rootDirectory, filename));
            return rootDirectory;
        }

        private async Task<string> CreateSampleImagesAsync(FileState state)
        {
            var rootDirectory = state.CreateTempDirectory();

            // Create sample files
            await CreateImage(Path.Combine(rootDirectory, "GOES16_FD_CH13_20200830T035020Z.jpg"));
            await CreateImage(Path.Combine(rootDirectory, "GOES16_FD_CH13_20200830T033020Z.jpg"));
            await CreateImage(Path.Combine(rootDirectory, "GOES16_FD_CH13_20200830T034020Z.jpg"));

            var directory = Directory.CreateDirectory(Path.Combine(rootDirectory, "GOES17"));
            await CreateImage(Path.Combine(directory.FullName, "GOES17_FD_CH13_20200830T033031Z.jpg"));
            await CreateImage(Path.Combine(directory.FullName, "GOES17_FD_CH13_20200830T044031Z.jpg"));

            directory = Directory.CreateDirectory(Path.Combine(rootDirectory, "Himawari-8"));
            await CreateImage(Path.Combine(directory.FullName, "Himawari8_FD_IR_20200830T034100Z.jpg"));
            await CreateImage(Path.Combine(directory.FullName, "Himawari8_FD_IR_20200830T045100Z.jpg"));
            await CreateImage(Path.Combine(directory.FullName, "IMG_FD_023_IR105_20200830_035006.jpg"));
            await CreateImage(Path.Combine(directory.FullName, "IMG_FD.jpg"));
            await CreateImage(Path.Combine(directory.FullName, "bogus.jpg"));

            return rootDirectory;
        }

        private static void VerifySuccessfulExecution(int returnCode) => returnCode.Should().Be(0, "program should have executed successfully");
        private static void VerifyFailedExecution(int returnCode) => returnCode.Should().Be(-1, "program should not have executed successfully");
    }
}