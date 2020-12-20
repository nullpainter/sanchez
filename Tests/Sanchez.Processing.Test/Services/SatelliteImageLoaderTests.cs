using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Sanchez.Processing.Models;
using Sanchez.Processing.Services;
using Sanchez.Processing.Test.Helper;
using Sanchez.Test.Common;

namespace Sanchez.Processing.Test.Services
{
    [TestFixture(TestOf = typeof(SatelliteImageLoader))]
    public class SatelliteImageLoaderTests : AbstractTests
    {
        private ISatelliteImageLoader ImageLoader => GetService<ISatelliteImageLoader>();
        private IFileService FileService => GetService<IFileService>();

        [Test]
        public async Task LoadImagesNoTimestamp()
        {
            await CreateSampleImagesAsync(State);

            var sourceFiles = FileService.GetSourceFiles();
            var registrations = FileService.ToRegistrations(sourceFiles, CancellationToken.None);
            
            // Run method under test
            var activity = ImageLoader.RegisterImages(registrations, RenderOptions.Timestamp);
            activity.Registrations.Select(i => Path.GetFileName(i.Path)).Should().BeEquivalentTo(
                "GOES16_FD_CH13_20200830T035020Z.jpg",
                "GOES16_FD_CH13_20200830T033020Z.jpg",
                "GOES17_FD_CH13_20200830T033031Z.jpg",
                "Himawari8_FD_IR_20200830T035100Z.jpg",
                "IMG_FD_023_IR105_20200830_035006.jpg"
            );

            await activity.LoadAllAsync();
            activity.Registrations.Select(i => i.Image).Should().NotContainNulls();

            GetDisplayNames(activity, "GOES16").Should().BeEquivalentTo("GOES-16");
            GetDisplayNames(activity, "GOES17").Should().BeEquivalentTo("GOES-17");
            GetDisplayNames(activity, "Himawari8").Should().BeEquivalentTo("Himawari-8");
            GetDisplayNames(activity, "IMG").Should().BeEquivalentTo("GEO-KOMPSAT-2A");
        }
        
        [Test]
        public async Task LoadImagesWithTimestamp()
        {
            await CreateSampleImagesAsync(State);
            RenderOptions.Timestamp = new DateTime(2020, 08, 30, 03, 30, 00, DateTimeKind.Utc);
            RenderOptions.Tolerance = TimeSpan.FromMinutes(30);

            var sourceFiles = FileService.GetSourceFiles();
            var registrations = FileService.ToRegistrations(sourceFiles, CancellationToken.None);

            // Run method under test
            var activity = ImageLoader.RegisterImages(registrations, RenderOptions.Timestamp);
            activity.Registrations.Select(i => Path.GetFileName(i.Path)).Should().BeEquivalentTo(
                "GOES16_FD_CH13_20200830T033020Z.jpg",
                "GOES17_FD_CH13_20200830T033031Z.jpg",
                "Himawari8_FD_IR_20200830T035100Z.jpg",
                "IMG_FD_023_IR105_20200830_035006.jpg"
            );

            await activity.LoadAllAsync();
            activity.Registrations.Select(i => i.Image).Should().NotContainNulls();
        }

        private async Task CreateSampleImagesAsync(FileState state)
        {
            var rootDirectory = state.CreateTempDirectory();

            RenderOptions.EquirectangularRender = new EquirectangularRenderOptions(true, false, true);
            RenderOptions.SourcePath = rootDirectory;

            // Create sample files
            await CreateImage(Path.Combine(rootDirectory, "GOES16_FD_CH13_20200830T035020Z.jpg"));
            await CreateImage(Path.Combine(rootDirectory, "GOES16_FD_CH13_20200830T033020Z.jpg"));

            var directory = Directory.CreateDirectory(Path.Combine(rootDirectory, "GOES17"));
            await CreateImage(Path.Combine(directory.FullName, "GOES17_FD_CH13_20200830T033031Z.jpg"));

            directory = Directory.CreateDirectory(Path.Combine(rootDirectory, "Himawari-8"));
            await CreateImage(Path.Combine(directory.FullName, "Himawari8_FD_IR_20200830T035100Z.jpg"));
            await CreateImage(Path.Combine(directory.FullName, "IMG_FD_023_IR105_20200830_035006.jpg"));

            await CreateImage(Path.Combine(directory.FullName, "bogus.jpg"));
        }

        private static IEnumerable<string> GetDisplayNames(Activity activity, string prefix)
        {
            return activity.Registrations
                .Where(i => Path.GetFileName(i.Path).StartsWith(prefix))
                .Select(i => i.Definition.DisplayName)
                .Distinct();
        }
    }
}