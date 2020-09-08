using System.IO;
using System.Linq;
using FluentAssertions;
using Funhouse.Services.Filesystem;
using Funhouse.Test.Helper;
using NUnit.Framework;

namespace Funhouse.Test.Filesystem
{
    [TestFixture(TestOf = typeof(ImageLocator))]
    public class ImageLocatorTests : AbstractTests
    {
        private IImageLocator Locator => GetService<IImageLocator>();

        [Test]
        public void LocateImages()
        {
            using var state = FileHelper.NewState();
            var rootDirectory = state.CreateTempDirectory();

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
            var matchedFiles = Locator.LocateImages(rootDirectory).Select(Path.GetFileName);
            matchedFiles.Should().BeEquivalentTo("GOES16_FD_CH13_20200830T033020Z.jpg", "GOES17_FD_CH13_20200830T033031Z.jpg", "Himawari8_FD_IR_20200830T035100Z.jpg");
        }
    }
}