using FluentAssertions;
using Funhouse.Models.Angles;
using Funhouse.Models.Configuration;
using MathNet.Spatial.Units;
using NUnit.Framework;

namespace Funhouse.Test
{
    [TestFixture(TestOf = typeof(ProjectionOverlapCalculator))]
    public class ProjectionOverlapCalculatorTests : AbstractTests
    {
        private void VerifyRangeEquivalency(Range first, Range second)
        {
            first.Start.Degrees.Should().BeApproximately(second.Start.Degrees, Precision);
            first.End.Degrees.Should().BeApproximately(second.End.Degrees, Precision);
        }

        [Test]
        public void NonOverlapping()
        {
            var goes16 = ToDefinition(-156.2995, 6.2995);
            var nonOverlapping = ToDefinition(60, 120);

            var calculator = new ProjectionOverlapCalculator(goes16, nonOverlapping);

            VerifyRangeEquivalency(calculator.GetNonOverlappingRange(goes16), goes16.VisibleRange);
            VerifyRangeEquivalency(calculator.GetNonOverlappingRange(nonOverlapping), nonOverlapping.VisibleRange);
        }

        [Test]
        public void OverlappingSingleWithWrap()
        {
            var first = ToDefinition(-150, 10);
            var second = ToDefinition(140, -50);

            var calculator = new ProjectionOverlapCalculator(first, second);

            VerifyRangeEquivalency(
                calculator.GetNonOverlappingRange(first),
                new Range(Angle.FromDegrees(-100), Angle.FromDegrees(10)).NormaliseLongitude());

            VerifyRangeEquivalency(
                calculator.GetNonOverlappingRange(second),
                new Range(Angle.FromDegrees(140), Angle.FromDegrees(-100)).NormaliseLongitude());
        }

        [Test]
        public void OverlappingSingleNoWrap()
        {
            var first = ToDefinition(-100, 0);
            var second = ToDefinition(-50, 150);

            var calculator = new ProjectionOverlapCalculator(first, second);

            VerifyRangeEquivalency(
                calculator.GetNonOverlappingRange(first),
                new Range(Angle.FromDegrees(-100), Angle.FromDegrees(-25)).NormaliseLongitude());

            VerifyRangeEquivalency(
                calculator.GetNonOverlappingRange(second),
                new Range(Angle.FromDegrees(-25), Angle.FromDegrees(150)).NormaliseLongitude());
        }

        [Test]
        public void OverlappingBoth()
        {
            // Overlapped by both second and third
            var first = ToDefinition(-150, 10);
            var second = ToDefinition(140, -50);
            var third = ToDefinition(0, 60);

            var calculator = new ProjectionOverlapCalculator(first, second, third);

            VerifyRangeEquivalency(
                calculator.GetNonOverlappingRange(first),
                new Range(Angle.FromDegrees(-100), Angle.FromDegrees(5)).NormaliseLongitude());

            VerifyRangeEquivalency(
                calculator.GetNonOverlappingRange(second),
                new Range(Angle.FromDegrees(140), Angle.FromDegrees(-100)).NormaliseLongitude());

            VerifyRangeEquivalency(
                calculator.GetNonOverlappingRange(third),
                new Range(Angle.FromDegrees(5), Angle.FromDegrees(60)).NormaliseLongitude());
        }


        private static SatelliteDefinition ToDefinition(double startDegrees, double endDegrees)
        {
            var nonOverlapping = new SatelliteDefinition("", "", new Angle(),
                new Range(
                    Angle.FromDegrees(startDegrees),
                    Angle.FromDegrees(endDegrees)),
                new ImageOffset(new Angle(), new Angle(), 0));
            return nonOverlapping;
        }
    }
}