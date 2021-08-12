using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Configuration;
using Sanchez.Processing.Models.Projections;
using Sanchez.Processing.Services.Underlay;
using Sanchez.Test.Common;
using SixLabors.ImageSharp;
using Range = Sanchez.Processing.Models.Angles.Range;

namespace Sanchez.Processing.Test.Services
{
    [TestFixture(TestOf = typeof(UnderlayService))]
    public class UnderlayServiceTests : AbstractTests
    {
        private IUnderlayService UnderlayService => GetService<IUnderlayService>();

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            RenderOptions.UnderlayPath = Constants.DefaultUnderlayPath;
        }

        [Test]
        public async Task EquirectangularUnderlay()
        {
            var (definition, _) = SatelliteRegistry.Locate(Goes16DefinitionPrefix);
            Assert.NotNull(definition, "Unable to find satellite definition");

            var data = new UnderlayProjectionData(
                ProjectionType.Equirectangular,
                InterpolationType.NearestNeighbour,
                "underlay.jpg",
                5424,
                new Size(5424, 5424),
                new Range(0, Math.PI / 2));

            RenderOptions.EquirectangularRender = new EquirectangularRenderOptions(false, false, false);
            var underlay = await UnderlayService.GetUnderlayAsync(data, definition);

            underlay.Width.Should().Be(5424);
            underlay.Height.Should().Be(5424);
        }
        
        [Test]
        public async Task EquirectangularUnderlayNoCrop()
        {
            var (definition, _) = SatelliteRegistry.Locate(Goes16DefinitionPrefix);
            Assert.NotNull(definition, "Unable to find satellite definition");

            var data = new UnderlayProjectionData(
                ProjectionType.Equirectangular,
                InterpolationType.NearestNeighbour,
                "underlay.jpg",
                1000,
                new Size(5424, 5424),
                new Range(0, Math.PI / 2));

            RenderOptions.EquirectangularRender = new EquirectangularRenderOptions(false, true, false);
            var underlay = await UnderlayService.GetUnderlayAsync(data, definition);

            underlay.Width.Should().Be(5424);
            underlay.Height.Should().Be(5424);
        }

        [Test]
        public async Task EquirectangularUnderlayWithCrop()
        {
            var (definition, _) = SatelliteRegistry.Locate(Goes16DefinitionPrefix);
            Assert.NotNull(definition, "Unable to find satellite definition");

            var options = new UnderlayProjectionData(
                ProjectionType.Equirectangular,
                InterpolationType.NearestNeighbour,
                "underlay.jpg",
                5424,
                latitudeCrop: new Range(Angle.FromDegrees(45), Angle.FromDegrees(-45)));

            RenderOptions.EquirectangularRender = new EquirectangularRenderOptions(false, false, false);
            var underlay = await UnderlayService.GetUnderlayAsync(options, definition);

            underlay.Width.Should().Be(10848);
            underlay.Height.Should().Be(2712);
        }

        [Test]
        public async Task GeostationaryUnderlay()
        {
            var (definition, _) = SatelliteRegistry.Locate(Goes16DefinitionPrefix);
            Assert.NotNull(definition, "Unable to find satellite definition");

            var options = new UnderlayProjectionData(
                ProjectionType.Geostationary,
                InterpolationType.NearestNeighbour, "underlay.jpg", 5424);

            var underlay = await UnderlayService.GetUnderlayAsync(options, definition);

            underlay.Width.Should().Be(5424);
            underlay.Height.Should().Be(5424);

            // Retrieve cached
            underlay = await UnderlayService.GetUnderlayAsync(options, definition);

            underlay.Width.Should().Be(5424);
            underlay.Height.Should().Be(5424);

            // Verify changing options doesn't retrieve cached underlay
            options = new UnderlayProjectionData(
                ProjectionType.Geostationary,
                InterpolationType.NearestNeighbour, "underlay.jpg", 5424);

            underlay = await UnderlayService.GetUnderlayAsync(options, definition);

            underlay.Width.Should().Be(5424);
            underlay.Height.Should().Be(5424);
        }
    }
}