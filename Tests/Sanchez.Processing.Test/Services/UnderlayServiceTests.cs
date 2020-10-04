using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Angles;
using Sanchez.Processing.Models.Configuration;
using Sanchez.Processing.Services.Underlay;
using Sanchez.Test.Common;

namespace Sanchez.Processing.Test.Services
{
    [TestFixture(TestOf = typeof(UnderlayService))]
    public class UnderlayServiceTests : AbstractTests
    {
        private IUnderlayService UnderlayService => GetService<IUnderlayService>();

        [Test]
        public async Task EquirectangularUnderlay()
        {
            var definition = SatelliteRegistry.Locate(Goes16DefinitionPrefix);
            Assert.NotNull(definition, "Unable to find satellite definition");

            var options = new UnderlayProjectionOptions(
                ProjectionType.Equirectangular,
                InterpolationType.NearestNeighbour,
                5424,
                Constants.DefaultUnderlayPath);

            var underlay = await UnderlayService.GetUnderlayAsync(options, definition);

            underlay.Width.Should().Be(10848);
            underlay.Height.Should().Be(5424);
        }

        [Test]
        public async Task EquirectangularUnderlayWithCrop()
        {
            var definition = SatelliteRegistry.Locate(Goes16DefinitionPrefix);
            Assert.NotNull(definition, "Unable to find satellite definition");

            var options = new UnderlayProjectionOptions(
                ProjectionType.Equirectangular,
                InterpolationType.NearestNeighbour,
                5424,
                Constants.DefaultUnderlayPath,
                latitudeCrop: new Range(Angle.FromDegrees(45), Angle.FromDegrees(-45)),
                longitudeCrop: new Range(Angle.FromDegrees(-100), Angle.FromDegrees(100)));

            var underlay = await UnderlayService.GetUnderlayAsync(options, definition);

            underlay.Width.Should().Be(10848);
            underlay.Height.Should().Be(2712);
        }

        [Test]
        public async Task GeostationaryUnderlay()
        {
            var definition = SatelliteRegistry.Locate(Goes16DefinitionPrefix);
            Assert.NotNull(definition, "Unable to find satellite definition");

            var options = new UnderlayProjectionOptions(
                ProjectionType.Geostationary,
                InterpolationType.NearestNeighbour, 5424,
                Constants.DefaultUnderlayPath, 1000);

            var underlay = await UnderlayService.GetUnderlayAsync(options, definition);

            underlay.Width.Should().Be(1000);
            underlay.Height.Should().Be(1000);

            // Retrieve cached
            underlay = await UnderlayService.GetUnderlayAsync(options, definition);

            underlay.Width.Should().Be(1000);
            underlay.Height.Should().Be(1000);

            // Verify changing options doesn't retrieve cached underlay
            options = new UnderlayProjectionOptions(
                ProjectionType.Geostationary,
                InterpolationType.NearestNeighbour, 5424,
                Constants.DefaultUnderlayPath, 1500);

            underlay = await UnderlayService.GetUnderlayAsync(options, definition);

            underlay.Width.Should().Be(1500);
            underlay.Height.Should().Be(1500);
        }
    }
}