using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Sanchez.Test.Helper;
using NUnit.Framework;
using Sanchez.Models;
using SixLabors.ImageSharp;

namespace Sanchez.Test
{
    [TestFixture(TestOf = typeof(Bootstrapper))]
    public class EndToEndTests : AbstractTests
    {
        [Test]
        public async Task EquirectangularSingle()
        {
            using var fileState = new FileState();
            const string sourceFile = "GOES17_FD_CH13_20200830T033031Z.jpg";

            var rootDirectory = await CreateSingleSimpleImageAsync(fileState, sourceFile);

            var outputDirectory = fileState.CreateTempDirectory();
            var outputFile = Path.Combine(outputDirectory, "out.jpg");

            await Bootstrapper.Main(
                "reproject",
                "-s", Path.Combine(rootDirectory, sourceFile),
                "-o", outputFile);

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

            await Bootstrapper.Main(
                "reproject",
                "-s", rootDirectory,
                "-o", outputDirectory,
                "--mode", "batch");
            
            Directory.Exists(outputDirectory).Should().BeTrue("output directory should have been created");
            Directory.GetFiles(outputDirectory).Should().HaveCount(5);

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

            await Bootstrapper.Main(
                "reproject",
                "-s", rootDirectory,
                "-o", outputFile,
                "--mode", "stitch",
                "-T", "2020-08-30T03:50:20");

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

            await Bootstrapper.Main(
                "reproject",
                "-s", rootDirectory,
                "-o", outputFile,
                "-a",
                "--mode", "stitch",
                "-T", "2020-08-30T03:50:20");

            File.Exists(outputFile).Should().BeTrue("output file should have been created");
            var outputImage = await Image.LoadAsync(outputFile);

            outputImage.Width.Should().Be(4066);
            outputImage.Height.Should().Be(1916);
        }

        [Test]
        public async Task EquirectangularSingleWithCrop()
        {
            using var fileState = new FileState();
            const string sourceFile = "GOES17_FD_CH13_20200830T033031Z.jpg";

            var rootDirectory = await CreateSingleSimpleImageAsync(fileState, sourceFile);

            var outputDirectory = fileState.CreateTempDirectory();
            var outputFile = Path.Combine(outputDirectory, "out.jpg");

            await Bootstrapper.Main(
                "reproject",
                "-s", Path.Combine(rootDirectory, sourceFile),
                "-o", outputFile,
                "-a");

            File.Exists(outputFile).Should().BeTrue("output file should have been created");
            var outputImage = await Image.LoadAsync(outputFile);
            outputImage.Width.Should().Be(1918);
            outputImage.Height.Should().Be(1756);
        }

        [Test]
        public async Task GeostationarySingle()
        {
            using var fileState = new FileState();
            var rootDirectory = await CreateSampleImagesAsync(fileState);
            var outputDirectory = fileState.CreateTempDirectory();
            var outputFile = Path.Combine(outputDirectory, "out.jpg");

            await Bootstrapper.Main(
                "-s", Path.Combine(rootDirectory, "GOES16_FD_CH13_20200830T035020Z.jpg"),
                "-o", outputFile,
                "-q");

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

            await Bootstrapper.Main(
                "-s", rootDirectory,
                "-o", outputDirectory,
                "-q");

            Directory.Exists(outputDirectory).Should().BeTrue("output directory should have been created");
            Directory.GetFiles(outputDirectory).Should().HaveCount(5);

            foreach (var outputFile in Directory.GetFiles(outputDirectory))
            {
                var outputImage = await Image.LoadAsync(outputFile);
                outputImage.Width.Should().Be(Constants.Satellite.ImageSize.FourKm);
                outputImage.Height.Should().Be(Constants.Satellite.ImageSize.FourKm);
            }
        }

        [Test]
        public async Task GeostationaryMultipleReprojected()
        {
            using var fileState = new FileState();
            var rootDirectory = await CreateSampleImagesAsync(fileState);
            var outputDirectory = fileState.CreateTempDirectory();
            var outputFile = Path.Combine(outputDirectory, "out.jpg");

            await Bootstrapper.Main(
                "-s", rootDirectory,
                "-o", outputFile,
                "-l", "174",
                "-T 2020-08-30T03:50:20",
                "-q");

            File.Exists(outputFile).Should().BeTrue("output file should have been created");
            var outputImage = await Image.LoadAsync(outputFile);
            outputImage.Width.Should().Be(Constants.Satellite.ImageSize.FourKm);
            outputImage.Height.Should().Be(Constants.Satellite.ImageSize.FourKm);
        }


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

            var directory = Directory.CreateDirectory(Path.Combine(rootDirectory, "GOES17"));
            await CreateImage(Path.Combine(directory.FullName, "GOES17_FD_CH13_20200830T033031Z.jpg"));

            directory = Directory.CreateDirectory(Path.Combine(rootDirectory, "Himawari-8"));
            await CreateImage(Path.Combine(directory.FullName, "Himawari8_FD_IR_20200830T035100Z.jpg"));
            await CreateImage(Path.Combine(directory.FullName, "IMG_FD_023_IR105_20200830_035006.jpg"));
            await CreateImage(Path.Combine(directory.FullName, "bogus.jpg"));

            return rootDirectory;
        }
    }
}