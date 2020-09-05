using FluentAssertions;
using Funhouse.Models;
using Funhouse.Models.Angles;
using NUnit.Framework;

namespace Funhouse.Test
{
    public class ProjectionAngleTests : AbstractTests
    {
        [Test]
        public void ToX()
        {
            ProjectionAngle.ToX(Angle.FromDegrees(-180).Radians, 200).Should().Be(0);
            ProjectionAngle.ToX(Angle.FromDegrees(180).Radians, 200).Should().Be(200);
            ProjectionAngle.ToX(Angle.FromDegrees(0).Radians, 200).Should().Be(100);
        }

        [Test]
        public void FromX()
        {
            Angle.FromRadians(ProjectionAngle.FromX(0, 200)).Degrees.Should().BeApproximately(-180, Precision);
            Angle.FromRadians(ProjectionAngle.FromX(200, 200)).Degrees.Should().BeApproximately(180, Precision);
            Angle.FromRadians(ProjectionAngle.FromX(100, 200)).Degrees.Should().BeApproximately(0, Precision);
        }

        [Test]
        public void FromY()
        {
            Angle.FromRadians(ProjectionAngle.FromY(0, 200)).Degrees.Should().BeApproximately(-90, Precision);
            Angle.FromRadians(ProjectionAngle.FromY(200, 200)).Degrees.Should().BeApproximately(90, Precision);
            Angle.FromRadians(ProjectionAngle.FromY(100, 200)).Degrees.Should().BeApproximately(0, Precision);
        }

        [Test]
        public void ToY()
        {
            ProjectionAngle.ToY(Angle.FromDegrees(-90).Radians, 200).Should().Be(0);
            ProjectionAngle.ToY(Angle.FromDegrees(90).Radians, 200).Should().Be(200);
            ProjectionAngle.ToY(Angle.FromDegrees(0).Radians, 200).Should().Be(100);
        }
    }
}