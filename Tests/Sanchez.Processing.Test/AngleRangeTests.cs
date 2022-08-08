using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Angles;
using Sanchez.Test.Common;

namespace Sanchez.Processing.Test;

[TestFixture(TestOf = typeof(AngleRange))]
public class AngleRangeTests : AbstractTests
{
    [Test]
    public void NormaliseLongitudeNotRequired()
    {
        var range = new AngleRange(Angle.FromDegrees(-156), Angle.FromDegrees(6));
        range.NormaliseLongitude();

        Angle.FromRadians(range.Start).Degrees.Should().BeApproximately(-156, Precision);
        Angle.FromRadians(range.End).Degrees.Should().BeApproximately(6, Precision);
    }

    [Test]
    public void NormaliseLongitude()
    {
        var range = new AngleRange(Angle.FromDegrees(190), Angle.FromDegrees(-190));
        range = range.NormaliseLongitude();

        Angle.FromRadians(range.Start).Degrees.Should().BeApproximately(-170, Precision);
        Angle.FromRadians(range.End).Degrees.Should().BeApproximately(170, Precision);
    }
}