using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Sanchez.Processing.Models;
using Sanchez.Processing.Services;
using Sanchez.Processing.Services.Filesystem;
using Sanchez.Processing.Test.Helper;
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
            RenderOptions.EquirectangularRender = new EquirectangularRenderOptions(false, true, null);

            var targetTimestamp = new DateTime(2020, 08, 30, 03, 30, 00, DateTimeKind.Utc);
            RenderOptions.Tolerance = TimeSpan.FromMinutes(30);

            using var state = new FileState();
            var rootDirectory = state.CreateTempDirectory();
            RenderOptions.SourcePath = rootDirectory;

            // Create sample files
            state.CreateFile(rootDirectory, "GOES16_FD_CH13_20200830T035020Z.jpg");
            state.CreateFile(rootDirectory, "GOES16_FD_CH13_20200830T033020Z.jpg");
            state.CreateFile(rootDirectory, "GOES16_FD_CH13_20200930T033020Z.jpg");
            state.CreateFile(rootDirectory, "GOES16_FD_CH13_bogus.jpg");

            var directory = Directory.CreateDirectory(Path.Combine(rootDirectory, "GOES17"));
            state.CreateFile(directory.FullName, "GOES17_FD_CH13_20200830T033031Z.jpg");

            directory = Directory.CreateDirectory(Path.Combine(rootDirectory, "Himawari-8"));
            state.CreateFile(directory.FullName, "Himawari8_FD_IR_20200830T035100Z.jpg");
            state.CreateFile(directory.FullName, "bogus.jpg");

            // Run method under test
            var sourceFiles = FileService.GetSourceFiles();
            var registrations = FileService.ToRegistrations(sourceFiles);
            
            var matchedFiles = Matcher.FilterMatchingRegistrations(registrations, targetTimestamp).Select(r => Path.GetFileName(r.Path));
            matchedFiles.Should().BeEquivalentTo("GOES16_FD_CH13_20200830T033020Z.jpg", "GOES17_FD_CH13_20200830T033031Z.jpg", "Himawari8_FD_IR_20200830T035100Z.jpg");
        }
    }
}