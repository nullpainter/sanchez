using FluentAssertions;
using NUnit.Framework;
using Sanchez.Processing.Extensions;
using Sanchez.Processing.Helpers;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Angles;
using Sanchez.Test.Common;

namespace Sanchez.Processing.Test
{
    public class ProjectionAngleTests : AbstractTests
    {
        [Test]
        public void ToX()
        {
            var pixelRange = new Range(Angle.FromDegrees(-180), Angle.FromDegrees(180)).ToPixelRangeX(200);
            
            pixelRange.Start.Should().Be(0);
            pixelRange.End.Should().Be(200);
        }

        [Test]
        public void FromX()
        {
            Angle.FromRadians(ProjectionAngleConverter.FromX(0, 200)).Degrees.Should().BeApproximately(-180, Precision);
            Angle.FromRadians(ProjectionAngleConverter.FromX(200, 200)).Degrees.Should().BeApproximately(180, Precision);
            Angle.FromRadians(ProjectionAngleConverter.FromX(100, 200)).Degrees.Should().BeApproximately(0, Precision);
        }

        [Test]
        public void FromY()
        {
            Angle.FromRadians(ProjectionAngleConverter.FromY(0, 200)).Degrees.Should().BeApproximately(90, Precision);
            Angle.FromRadians(ProjectionAngleConverter.FromY(200, 200)).Degrees.Should().BeApproximately(-90, Precision);
            Angle.FromRadians(ProjectionAngleConverter.FromY(100, 200)).Degrees.Should().BeApproximately(0, Precision);
        }
        
        [Test]
        public void ToYSouthernHemisphere()
        {
            var pixelRange = new Range(Angle.FromDegrees(-3), Angle.FromDegrees(-53)).ToPixelRangeY(200);
            
            pixelRange.Start.Should().Be(103);
            pixelRange.End.Should().Be(159);
        }

        [Test]
        public void ToY()
        {
            var pixelRange = new Range(Angle.FromDegrees(90), Angle.FromDegrees(-90)).ToPixelRangeY(200);
            
            pixelRange.Start.Should().Be(0);
            pixelRange.End.Should().Be(200);
        }
    }
}