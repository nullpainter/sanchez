using FluentAssertions;
using Funhouse.Models.Angles;
using MathNet.Spatial.Units;
using NUnit.Framework;

namespace Funhouse.Test
{
    [TestFixture(TestOf = typeof(Range))]
    public class RangeTests : AbstractTests
    {
        [Test]
        public void NormaliseLongitudeNotRequired()
        {
            var range = new Range(Angle.FromDegrees(-156), Angle.FromDegrees(6));
            range.NormaliseLongitude();

            range.Start.Degrees.Should().BeApproximately(-156, Precision);
            range.End.Degrees.Should().BeApproximately(6, Precision);
        }

        [Test]
        public void NormaliseLongitude()
        {
            var range = new Range(Angle.FromDegrees(141), Angle.FromDegrees(-55));
            range.NormaliseLongitude();

            range.Start.Degrees.Should().BeApproximately(141, Precision);
            range.End.Degrees.Should().BeApproximately(305, Precision);
        }
    }
}