using System;
using FluentAssertions;
using NUnit.Framework;
using Sanchez.Models;
using Sanchez.Projections;
using static Sanchez.Models.Angle;

namespace Sanchez.Test.Projections
{
    [TestFixture(TestOf = typeof(GeostationaryProjection))]
    public class GeostationaryProjectionTests : AbstractTests
    {
        private const float PixelPrecision = 1.0f;

        [Test]
        [Description("Sample equation from GOES-R documentation")]
        public void TextbookToScanningAngle()
        {
            var definition = SatelliteRegistry.Locate(Goes16DefinitionPrefix);
            Assert.NotNull(definition, "Unable to find satellite definition");

            GeostationaryProjection.ToScanningAngle(
                FromDegrees(33.846162).Radians,
                FromDegrees(-84.690932).Radians,
                definition,
                out var scanningX,
                out var scanningY);

            scanningX.Should().BeApproximately(-0.024052, Precision);
            scanningY.Should().BeApproximately(0.0953399, Precision);
        }

        [Test]
        public void ProjectionFernandinaIsland()
        {
            var definition = SatelliteRegistry.Locate(Goes17DefinitionPrefix);
            Assert.NotNull(definition, "Unable to find satellite definition");
            
            var imageOffset = Constants.Satellite.Offset.TwoKm;

            const int x = 4866;
            const int y = 2735;

            // Convert from scanning angle to geodetic
            var scanningX = imageOffset.ToHorizontalScanningAngle(x);
            var scanningY = imageOffset.ToVerticalScanningAngle(y);

            ReverseGeostationaryProjection.ToLatitudeLongitude(scanningX, scanningY, definition.Longitude, definition.Height, out var latitude, out var longitude);

            FromRadians(latitude).Degrees.Should().BeApproximately(-0.4489090116682934, Precision);
            FromRadians(longitude).Degrees.Should().BeApproximately(-91.39243691622593, Precision);
        }

        [Test]
        public void ProjectionLakeTaupo()
        {
            var definition = SatelliteRegistry.Locate(Goes17DefinitionPrefix);
            Assert.NotNull(definition, "Unable to find satellite definition");
            
            var imageOffset = Constants.Satellite.Offset.TwoKm;

            const int x = 1050;
            const int y = 4533;

            // Convert from scanning angle to geodetic
            var scanningX = imageOffset.ToHorizontalScanningAngle(x);
            var scanningY = imageOffset.ToVerticalScanningAngle(y);

            ReverseGeostationaryProjection.ToLatitudeLongitude(scanningX, scanningY, definition.Longitude, definition.Height,  out var latitude, out var longitude);

            FromRadians(latitude).Degrees.Should().BeApproximately(-38.70625734950147, Precision);
            FromRadians(longitude).Degrees.Should().BeApproximately(175.9626023681634, Precision);

            // Convert back to scanning angle
            GeostationaryProjection.ToScanningAngle(latitude, longitude, definition, out var scanningXResult, out var scanningYResult);
            scanningXResult.Should().BeApproximately(scanningX, Precision);
            scanningYResult.Should().BeApproximately(scanningY, Precision);
            
            imageOffset.ToImageCoordinates(scanningXResult, scanningYResult, out var targetX, out var targetY);

            Math.Round(targetX).Should().BeApproximately(x, PixelPrecision);
            Math.Round(targetY).Should().BeApproximately(y, PixelPrecision);
        }

        [Test]
        public void ProjectionKauai()
        {
            var definition = SatelliteRegistry.Locate(Goes17DefinitionPrefix);
            Assert.NotNull(definition, "Unable to find satellite definition");
       
            var imageOffset = Constants.Satellite.Offset.TwoKm;

            const int x = 1614;
            const int y = 1564;

            // Convert from scanning angle to geodetic
            var scanningX = imageOffset.ToHorizontalScanningAngle(x);
            var scanningY = imageOffset.ToVerticalScanningAngle(y);

            ReverseGeostationaryProjection.ToLatitudeLongitude(scanningX, scanningY, definition.Longitude, definition.Height,  out var latitude, out var longitude);

            FromRadians(latitude).Degrees.Should().BeApproximately(21.869911403254573, Precision);
            FromRadians(longitude).Degrees.Should().BeApproximately(-159.66206692006023, Precision);

            // Convert back to scanning angle
            GeostationaryProjection.ToScanningAngle(latitude, longitude, definition, out var scanningXResult, out var scanningYResult);
            scanningXResult.Should().BeApproximately(scanningX, Precision);
            scanningYResult.Should().BeApproximately(scanningY, Precision);
            
            imageOffset.ToImageCoordinates(scanningXResult, scanningYResult, out var targetX, out var targetY);

            Math.Round(targetX).Should().BeApproximately(x, PixelPrecision);
            Math.Round(targetY).Should().BeApproximately(y, PixelPrecision);
        }
        
        [Test]
        public void ProjectionCalifornia()
        {
            var definition = SatelliteRegistry.Locate(Goes17DefinitionPrefix);
            Assert.NotNull(definition, "Unable to find satellite definition");
       
            var imageOffset = Constants.Satellite.Offset.TwoKm;

            const int x = 3717;
            const int y = 1275;

            // Convert from scanning angle to geodetic
            var scanningX = imageOffset.ToHorizontalScanningAngle(x);
            var scanningY = imageOffset.ToVerticalScanningAngle(y);

            ReverseGeostationaryProjection.ToLatitudeLongitude(scanningX, scanningY, definition.Longitude, definition.Height, out var latitude, out var longitude);

            FromRadians(latitude).Degrees.Should().BeApproximately(28.007393788242464, Precision);
            FromRadians(longitude).Degrees.Should().BeApproximately(-115.44783176403854, Precision);

            // Convert back to scanning angle
            GeostationaryProjection.ToScanningAngle(latitude, longitude, definition, out var scanningXResult, out var scanningYResult);
            scanningXResult.Should().BeApproximately(scanningX, Precision);
            scanningYResult.Should().BeApproximately(scanningY, Precision);
            
            imageOffset.ToImageCoordinates(scanningXResult, scanningYResult, out var targetX, out var targetY);

            Math.Round(targetX).Should().BeApproximately(x, PixelPrecision);
            Math.Round(targetY).Should().BeApproximately(y, PixelPrecision);
        }
        
        [Test]
        public void TextbookToScanningAngleCentre()
        {
            var definition = SatelliteRegistry.Locate(Goes17DefinitionPrefix);
            Assert.NotNull(definition);

            var scanningX = Constants.Satellite.Offset.TwoKm.ToHorizontalScanningAngle(2712);
            var scanningY = Constants.Satellite.Offset.TwoKm.ToVerticalScanningAngle(2711);

            scanningX.Should().BeApproximately(0.000028, Precision);
            scanningY.Should().BeApproximately(0.000028, Precision);
        }

        [Test]
        public void ToScanningAngleTopLeft()
        {
            var definition = SatelliteRegistry.Locate(Goes17DefinitionPrefix);
            Assert.NotNull(definition);

            var scanningX = Constants.Satellite.Offset.TwoKm.ToHorizontalScanningAngle(0);
            var scanningY = Constants.Satellite.Offset.TwoKm.ToVerticalScanningAngle(0);

            scanningX.Should().BeApproximately(-0.151844, Precision);
            scanningY.Should().BeApproximately(0.151844, Precision);
        }

        [Test]
        public void ToScanningAngleBottomRight()
        {
            var definition = SatelliteRegistry.Locate(Goes17DefinitionPrefix);
            Assert.NotNull(definition);

            var scanningX = Constants.Satellite.Offset.TwoKm.ToHorizontalScanningAngle(5423);
            var scanningY = Constants.Satellite.Offset.TwoKm.ToVerticalScanningAngle(5423);

            scanningX.Should().BeApproximately(0.151844, Precision);
            scanningY.Should().BeApproximately(-0.151844, Precision);
        }

        [Test]
        [Description("Sample equation from GOES-R documentation")]
        public void TextbookToImageCoordinates()
        {
            var definition = SatelliteRegistry.Locate(Goes17DefinitionPrefix);
            Assert.NotNull(definition, "Cannot find satellite definition");

            var imageOffset = Constants.Satellite.Offset.TwoKm;
            imageOffset.ToImageCoordinates(0.000028, 0.000028, out var x, out var y);
            x.Should().BeApproximately(2712f, Precision);
            y.Should().BeApproximately(2711f, Precision);
        }
    }
}