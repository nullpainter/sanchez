using System;
using FluentAssertions;
using Funhouse.Models.Angles;
using Funhouse.Projections;
using NUnit.Framework;
using SixLabors.ImageSharp;
using static MathNet.Spatial.Units.Angle;

namespace Funhouse.Test.Projections
{
    [TestFixture(TestOf = typeof(GeostationaryProjection))]
    public class GeostationaryProjectionTests : AbstractTests
    {
        private const float PixelPrecision = 1.0f;

        [Test]
        [Description("Sample equation from GOES-R documentation")]
        public void TextbookFromGeodetic()
        {
            var definition = SatelliteRegistry.Locate("GOES16");
            Assert.NotNull(definition, "Unable to find satellite definition");

            var geodetic = new GeodeticAngle(FromDegrees(33.846162), FromDegrees(-84.690932));
            var scanningAngle = GeostationaryProjection.FromGeodetic(geodetic, definition);
            Assert.NotNull(scanningAngle);

            scanningAngle.Value.X.Radians.Should().BeApproximately(-0.024052, Precision);
            scanningAngle.Value.Y.Radians.Should().BeApproximately(-0.095340, Precision);
        }

        [Test]
        public void ProjectionFernandinaIsland()
        {
            var definition = SatelliteRegistry.Locate("GOES17");
            Assert.NotNull(definition, "Unable to find satellite definition");

            const int x = 4866;
            const int y = 2735;
            var scanningAngle = GeostationaryProjection.ToScanningAngle(new PointF(x, y), definition);

            // Convert from scanning angle to geodetic
            var angle = GeostationaryProjection.ToGeodetic(scanningAngle, definition);

            angle.Latitude.Degrees.Should().BeApproximately(-0.4489090116682934, Precision);
            angle.Longitude.Degrees.Should().BeApproximately(-91.39243691622595, Precision);

            // Convert back to scanning angle
            var scanningAngleResult = GeostationaryProjection.FromGeodetic(angle, definition);
            Assert.NotNull(scanningAngleResult);

            var (targetX, targetY) = GeostationaryProjection.ToImageCoordinates(scanningAngleResult.Value, definition);

            Math.Round(targetX).Should().BeApproximately(x, PixelPrecision);
            Math.Round(targetY).Should().BeApproximately(y, PixelPrecision);
        }

        [Test]
        public void ProjectionLakeTaupo()
        {
            var definition = SatelliteRegistry.Locate("GOES17");
            Assert.NotNull(definition, "Unable to find satellite definition");

            const int x = 1050;
            const int y = 4533;
            var scanningAngle = GeostationaryProjection.ToScanningAngle(new PointF(x, y), definition);
            var angle = GeostationaryProjection.ToGeodetic(scanningAngle, definition);

            angle.Latitude.Degrees.Should().BeApproximately(-38.70625734950147, Precision);
            angle.Longitude.Degrees.Should().BeApproximately(-184.0373976318366, Precision);

            var scanningAngleResult = GeostationaryProjection.FromGeodetic(angle, definition);
            Assert.NotNull(scanningAngleResult);

            var (targetX, targetY) = GeostationaryProjection.ToImageCoordinates(scanningAngleResult.Value, definition);

            Math.Round(targetX).Should().BeApproximately(x, PixelPrecision);
            Math.Round(targetY).Should().BeApproximately(y, PixelPrecision);
        }

        [Test]
        public void ProjectionKauai()
        {
            var definition = SatelliteRegistry.Locate("GOES17");
            Assert.NotNull(definition, "Unable to find satellite definition");

            const int x = 1614;
            const int y = 1564;

            var scanningAngle = GeostationaryProjection.ToScanningAngle(new PointF(x, y), definition);
            var angle = GeostationaryProjection.ToGeodetic(scanningAngle, definition);

            angle.Latitude.Degrees.Should().BeApproximately(21.869911403254573, Precision);
            angle.Longitude.Degrees.Should().BeApproximately(-159.66206692006023, Precision);

            var scanningAngleResult = GeostationaryProjection.FromGeodetic(angle, definition);
            Assert.NotNull(scanningAngleResult);

            var (targetX, targetY) = GeostationaryProjection.ToImageCoordinates(scanningAngleResult.Value, definition);

            Math.Round(targetX).Should().BeApproximately(x, PixelPrecision);
            Math.Round(targetY).Should().BeApproximately(y, PixelPrecision);
        }

        [Test]
        public void ProjectionCalifornia()
        {
            var definition = SatelliteRegistry.Locate("GOES17");
            Assert.NotNull(definition, "Unable to find satellite definition");

            const int x = 3717;
            const int y = 1275;

            var scanningAngle = GeostationaryProjection.ToScanningAngle(new PointF(x, y), definition);
            var angle = GeostationaryProjection.ToGeodetic(scanningAngle, definition);

            angle.Latitude.Degrees.Should().BeApproximately(28.007393788242464, Precision);
            angle.Longitude.Degrees.Should().BeApproximately(-115.44783176403854, Precision);

            var scanningAngleResult = GeostationaryProjection.FromGeodetic(angle, definition);
            Assert.NotNull(scanningAngleResult);

            var (targetX, targetY) = GeostationaryProjection.ToImageCoordinates(scanningAngleResult.Value, definition);

            Math.Round(targetX).Should().BeApproximately(x, PixelPrecision);
            Math.Round(targetY).Should().BeApproximately(y, PixelPrecision);
        }

        [Test]
        public void ToScanningAngleCentre()
        {
            var coordinates = new PointF(2712f, 2712f);
            var definition = SatelliteRegistry.Locate("GOES17");
            Assert.NotNull(definition);

            var scanningAngle = GeostationaryProjection.ToScanningAngle(coordinates, definition);

            scanningAngle.X.Radians.Should().BeApproximately(0.000028, Precision);
            scanningAngle.Y.Radians.Should().BeApproximately(0.000028, Precision);
        }

        [Test]
        public void ToScanningAngleTopLeft()
        {
            var coordinates = new PointF(0, 0);
            var definition = SatelliteRegistry.Locate("GOES17");
            Assert.NotNull(definition);

            var scanningAngle = GeostationaryProjection.ToScanningAngle(coordinates, definition);

            scanningAngle.X.Radians.Should().BeApproximately(-0.151844, Precision);
            scanningAngle.Y.Radians.Should().BeApproximately(-0.151844, Precision);
        }

        [Test]
        public void ToScanningAngleBottomRight()
        {
            var coordinates = new PointF(5424, 5424);
            var definition = SatelliteRegistry.Locate("GOES17");
            Assert.NotNull(definition);

            var scanningAngle = GeostationaryProjection.ToScanningAngle(coordinates, definition);

            scanningAngle.X.Radians.Should().BeApproximately(0.1519, Precision);
            scanningAngle.Y.Radians.Should().BeApproximately(0.1519, Precision);
        }

        [Test]
        public void ToImageCoordinates()
        {
            var scanningAngle = new ScanningAngle(FromRadians(0.000028), FromRadians(0.000028));

            var definition = SatelliteRegistry.Locate("GOES17");
            Assert.NotNull(definition);

            var coordinates = GeostationaryProjection.ToImageCoordinates(scanningAngle, definition);
            coordinates.X.Should().BeApproximately(2712f, Precision);
            coordinates.Y.Should().BeApproximately(2712f, Precision);
        }
    }
}