using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Funhouse.Models.Angles;
using Funhouse.Projections;
using Funhouse.Services;
using NUnit.Framework;
using SixLabors.ImageSharp;
using static MathNet.Spatial.Units.Angle;

namespace Funhouse.Test
{
    public class GeostationaryProjectionTests
    {
        private const float AnglePrecision = 0.000001f;
        private const float PixelPrecision = 1.0f;
        
        private readonly SatelliteRegistry _registry = new SatelliteRegistry(Path.Combine(TestContext.CurrentContext.TestDirectory, Constants.DefinitionsPath));

        [SetUp]
        public async Task SetupAsync()
        {
            await _registry.InitialiseAsync();
        }

        [Test]
        [Description("Sample equation from GOES-R documentation")]
        public void TextbookFromGeodetic()
        {
            var definition = _registry.LocateByPrefix("GOES16");
            Assert.NotNull(definition, "Unable to find satellite definition");
            
            var geodetic = new GeodeticAngle(FromDegrees(33.846162), FromDegrees(-84.690932));
            var scanningAngle = GeostationaryProjection.FromGeodetic(geodetic, definition);
            Assert.NotNull(scanningAngle);

            scanningAngle.Value.X.Radians.Should().BeApproximately(-0.024052, AnglePrecision);
            scanningAngle.Value.Y.Radians.Should().BeApproximately(0.095340, AnglePrecision);
        }

        [Test]
        public void ProjectionFernandinaIsland()
        {
            var definition = _registry.LocateByPrefix("GOES17");
            Assert.NotNull(definition, "Unable to find satellite definition");
            
            const int x = 4866;
            const int y = 2735;

            var angle = GeostationaryProjection.ToGeodetic(new PointF(x, y), definition);

            angle.Latitude.Degrees.Should().BeApproximately(-0.44890905925785773, AnglePrecision);
            angle.Longitude.Degrees.Should().BeApproximately(-91.39243066827123, AnglePrecision);

            var scanningAngle = GeostationaryProjection.FromGeodetic(angle, definition);
            Assert.NotNull(scanningAngle);

            var (targetX, targetY) = GeostationaryProjection.ToImageCoordinates(scanningAngle.Value, definition);

            Math.Round(targetX).Should().BeApproximately(x, PixelPrecision);
            Math.Round(targetY).Should().BeApproximately(y, PixelPrecision);
        }

        [Test]
        public void ProjectionLakeTaupo()
        {
            var definition = _registry.LocateByPrefix("GOES17");
            Assert.NotNull(definition, "Unable to find satellite definition");
            
            const int x = 1050;
            const int y = 4533;
            var angle = GeostationaryProjection.ToGeodetic(new PointF(x, y), definition);

            angle.Latitude.Degrees.Should().BeApproximately(-38.72867554863465, AnglePrecision);
            angle.Longitude.Degrees.Should().BeApproximately(-184.08642118115725, AnglePrecision);

            var scanningAngle = GeostationaryProjection.FromGeodetic(angle, definition);
            Assert.NotNull(scanningAngle);

            var (targetX, targetY) = GeostationaryProjection.ToImageCoordinates(scanningAngle.Value, definition);
            Console.WriteLine(targetX + "," + targetY);

            Math.Round(targetX).Should().BeApproximately(x, PixelPrecision);
            Math.Round(targetY).Should().BeApproximately(y, PixelPrecision);
        }

        [Test]
        public void ProjectionKauai()
        {
            var definition = _registry.LocateByPrefix("GOES17");
            Assert.NotNull(definition, "Unable to find satellite definition");
            
            const int x = 1614;
            const int y = 1564;
            var angle = GeostationaryProjection.ToGeodetic(new PointF(x, y), definition);

            angle.Latitude.Degrees.Should().BeApproximately(21.877129718937088, AnglePrecision);
            angle.Longitude.Degrees.Should().BeApproximately(-159.6707005235485, AnglePrecision);

            var scanningAngle = GeostationaryProjection.FromGeodetic(angle, definition);
            Assert.NotNull(scanningAngle);

            var (targetX, targetY) = GeostationaryProjection.ToImageCoordinates(scanningAngle.Value, definition);

            Math.Round(targetX).Should().BeApproximately(x, PixelPrecision);
            Math.Round(targetY).Should().BeApproximately(y, PixelPrecision);
        }

        [Test]
        public void ProjectionCalifornia()
        {
            var definition = _registry.LocateByPrefix("GOES17");
            Assert.NotNull(definition, "Unable to find satellite definition");
            
            const int x = 3717;
            const int y = 1275;

            var angle = GeostationaryProjection.ToGeodetic(new PointF(x, y), definition);

            angle.Latitude.Degrees.Should().BeApproximately(28.021721318519916, AnglePrecision);
            angle.Longitude.Degrees.Should().BeApproximately(-115.4340404140721, AnglePrecision);

            var scanningAngle = GeostationaryProjection.FromGeodetic(angle, definition);
            Assert.NotNull(scanningAngle);

            var (targetX, targetY) = GeostationaryProjection.ToImageCoordinates(scanningAngle.Value, definition);

            Math.Round(targetX).Should().BeApproximately(x, PixelPrecision);
            Math.Round(targetY).Should().BeApproximately(y, PixelPrecision);
        }
    }
}