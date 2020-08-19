using FluentAssertions;
using Funhouse.Models.Angles;
using NUnit.Framework;
using SixLabors.ImageSharp;

namespace Funhouse.Test
{
    [TestFixture(TestOf = typeof(ProjectionAngle))]
    public class ProjectionAngleTests : AbstractTests
    {
        [TestCase(100, 100, 200, 200, 0, 0)]
        [TestCase(0, 100, 200, 200, -180, 0)]
        [TestCase(200, 100, 200, 200, 180, 0)]
        [TestCase(100, 0, 200, 200, 0, -90)]
        [TestCase(100, 200, 200, 200, 0, 90)]
        public void FromPixelCoordinates(int x, int y, int width, int height, int expectedXAngle, int expectedYAngle)
        {
            var angle = ProjectionAngle.FromPixelCoordinates(new Point(x, y), width, height);
            angle.X.Degrees.Should().Be(expectedXAngle);
            angle.Y.Degrees.Should().Be(expectedYAngle);

            // Verify inverse
            var pixelCoordinates = angle.ToPixelCoordinates(width, height);
            pixelCoordinates.X.Should().BeApproximately(x, Precision);
            pixelCoordinates.Y.Should().BeApproximately(y, Precision);
        }
    }
}