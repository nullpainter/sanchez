using FluentAssertions;
using Funhouse.Models;
using Funhouse.Models.Angles;
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

            Angle.FromRadians(range.Start).Degrees.Should().BeApproximately(-156, Precision);
            Angle.FromRadians(range.End).Degrees.Should().BeApproximately(6, Precision);
        }

        [Test]
        public void NormaliseLongitude()
        {
            var range = new Range(Angle.FromDegrees(190), Angle.FromDegrees(-190));
            range = range.NormaliseLongitude();

            Angle.FromRadians(range.Start).Degrees.Should().BeApproximately(-170, Precision);
            Angle.FromRadians(range.End).Degrees.Should().BeApproximately(170, Precision);
        }
    }
}