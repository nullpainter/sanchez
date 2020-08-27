using FluentAssertions;
using Funhouse.Models.Angles;
using MathNet.Spatial.Units;
using NUnit.Framework;

namespace Funhouse.Test
{
    public class ProjectionAngleTests : AbstractTests
    {
        [Test]
        public void FromX()
        {
            ProjectionAngle.ToX(Angle.FromDegrees(-180), 200).Should().Be(0);
            ProjectionAngle.ToX(Angle.FromDegrees(180), 200).Should().Be(200);
            ProjectionAngle.ToX(Angle.FromDegrees(0), 200).Should().Be(100);
        }

        [Test]
        public void ToX()
        {
            ProjectionAngle.FromX(0, 200).Degrees.Should().Be(-180);
            ProjectionAngle.FromX(200, 200).Degrees.Should().Be(180);
            ProjectionAngle.FromX(100, 200).Degrees.Should().Be(0);
        }

        [Test]
        public void FromY()
        {
            ProjectionAngle.FromY(0, 200).Degrees.Should().Be(-90);
            ProjectionAngle.FromY(200, 200).Degrees.Should().Be(90);
            ProjectionAngle.FromY(100, 200).Degrees.Should().Be(0);
        }

        [Test]
        public void ToY()
        {
            ProjectionAngle.ToY(Angle.FromDegrees(-90), 200).Should().Be(0);
            ProjectionAngle.ToY(Angle.FromDegrees(90), 200).Should().Be(200);
            ProjectionAngle.ToY(Angle.FromDegrees(0), 200).Should().Be(100);
        }
    }
}