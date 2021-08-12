using System;
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Sanchez.Processing.Models;
using Sanchez.Processing.Services;
using Sanchez.Processing.Services.Filesystem;
using Sanchez.Test.Common;

namespace Sanchez.Processing.Test.Filesystem
{
    [TestFixture(TestOf = typeof(ImageMatcher))]
    public class ImageLocatorTests : AbstractTests
    {
        private IImageMatcher Matcher => GetService<IImageMatcher>();
        private IFileService FileService => GetService<IFileService>();

        [Test]
        public void LocateImages()
        {
            RenderOptions.EquirectangularRender = new EquirectangularRenderOptions(false, false, true, null);

            var targetTimestamp = new DateTime(2020, 08, 30, 03, 30, 00, DateTimeKind.Utc);
            RenderOptions.Tolerance = TimeSpan.FromMinutes(30);

            var rootDirectory = State.CreateTempDirectory();
            RenderOptions.SourcePath = rootDirectory;

            // Create sample files
            State.CreateFile(rootDirectory, "GOES16_FD_CH13_20200830T035020Z.jpg");
            State.CreateFile(rootDirectory, "GOES16_FD_CH13_20200830T033020Z.jpg");
            State.CreateFile(rootDirectory, "GOES16_FD_CH13_20200930T033020Z.jpg");
            State.CreateFile(rootDirectory, "GOES16_FD_CH13_bogus.jpg");

            var directory = Directory.CreateDirectory(Path.Combine(rootDirectory, "GOES17"));
            State.CreateFile(directory.FullName, "GOES17_FD_CH13_20200830T033031Z.jpg");

            directory = Directory.CreateDirectory(Path.Combine(rootDirectory, "Himawari-8"));
            State.CreateFile(directory.FullName, "Himawari8_FD_IR_20200830T035100Z.jpg");
            State.CreateFile(directory.FullName, "bogus.jpg");

            directory = Directory.CreateDirectory(Path.Combine(rootDirectory, "GK-2A"));
            State.CreateFile(directory.FullName, "IMG_FD_020_IR105_202008307_032006_ENHANCED.png");
            State.CreateFile(directory.FullName, "IMG_FD_020_IR105_20200830_032006.jpg");

            directory = Directory.CreateDirectory(Path.Combine(rootDirectory, "EWS-G1"));
            State.CreateFile(directory.FullName, "G13_4_20210830T033020Z.png");
            State.CreateFile(directory.FullName, "EWS-G1_2_20200830T033020Z.png");

            // Run method under test
            var sourceFiles = FileService.GetSourceFiles();
            var registrations = FileService.ToRegistrations(sourceFiles, CancellationToken.None);

            // Verify all valid files are matched
            registrations.Select(r => Path.GetFileName(r.Path)).Should().BeEquivalentTo(
                "EWS-G1_2_20200830T033020Z.png", "G13_4_20210830T033020Z.png", "IMG_FD_020_IR105_20200830_032006.jpg",
                "GOES16_FD_CH13_20200830T033020Z.jpg", "GOES16_FD_CH13_20200830T035020Z.jpg", "GOES16_FD_CH13_20200930T033020Z.jpg", "GOES17_FD_CH13_20200830T033031Z.jpg",
                "Himawari8_FD_IR_20200830T035100Z.jpg");

            // Verify closest image by timestamp, per satellite, is matched
            var matchedFiles = Matcher.FilterMatchingRegistrations(registrations, targetTimestamp).Select(r => Path.GetFileName(r.Path));
            matchedFiles.Should().BeEquivalentTo("GOES16_FD_CH13_20200830T033020Z.jpg", "GOES17_FD_CH13_20200830T033031Z.jpg", "Himawari8_FD_IR_20200830T035100Z.jpg",
                "IMG_FD_020_IR105_20200830_032006.jpg", "EWS-G1_2_20200830T033020Z.png");
        }
    }
}