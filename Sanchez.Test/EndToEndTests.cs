﻿using System.IO;
using FluentAssertions;
using NUnit.Framework;
using Sanchez.Test.Helper;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Sanchez.Test
{
    [TestFixture(TestOf = typeof(Sanchez))]
    public class EndToEndTests
    {
        [Test]
        public void WithMask()
        {
            using var fileState = FileHelper.NewState();
            var tempDirectory = fileState.CreateTempDirectory();

            const string maskFilename = "mask.jpg";
            const string underlayFilename = "underlay.jpg";
            const string satelliteFilename = "satellite.jpg";
            const string outputFilename = "output.jpg";

            CreateImage(tempDirectory, maskFilename);
            CreateImage(tempDirectory, underlayFilename);
            CreateImage(tempDirectory, satelliteFilename);

            var outputPath = Path.Combine(tempDirectory, outputFilename);

            // Run method under test
            Sanchez.Main(
                "-s", Path.Combine(tempDirectory, satelliteFilename),
                "-m", Path.Combine(tempDirectory, maskFilename),
                "-u", Path.Combine(tempDirectory, underlayFilename),
                "-o", outputPath,
                "-t", "00BBFF"
            );

            File.Exists(outputPath).Should().BeTrue("output file should have been created");
            using var outputImage = Image.Load(outputPath);
            outputImage.Width.Should().Be(2000);
            outputImage.Height.Should().Be(2000);
        }

        private static void CreateImage(string tempDirectory, string filename)
        {
            var path = Path.Combine(tempDirectory, filename);

            using var image = new Image<Rgba32>(2000, 2000);
            using var stream = new FileStream(path, FileMode.Create);
            
            image.SaveAsJpeg(stream);
        }

        [Test]
        public void WithoutMask()
        {
            using var fileState = FileHelper.NewState();
            var tempDirectory = fileState.CreateTempDirectory();

            const string maskFilename = "mask.jpg";
            const string underlayFilename = "underlay.jpg";
            const string satelliteFilename = "satellite.jpg";
            const string outputFilename = "output.jpg";

            CreateImage(tempDirectory, maskFilename);
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
    }
}