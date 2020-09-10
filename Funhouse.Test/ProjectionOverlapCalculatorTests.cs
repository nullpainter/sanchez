using System.Collections.Generic;
using FluentAssertions;
using Funhouse.Models;
using Funhouse.Models.Angles;
using Funhouse.Models.Configuration;
using Funhouse.Services;
using Funhouse.Services.Filesystem;
using NUnit.Framework;

namespace Funhouse.Test
{
    [TestFixture(TestOf = typeof(ProjectionOverlapCalculator))]
    public class ProjectionOverlapCalculatorTests : AbstractTests
    {
        private void VerifyRangeEquivalency(Range first, Range second)
        {
            Angle.FromRadians(first.Start).Degrees.Should().BeApproximately(Angle.FromRadians(second.Start).Degrees, Precision);
            Angle.FromRadians(first.End).Degrees.Should().BeApproximately(Angle.FromRadians(second.End).Degrees, Precision);
        }
        
        [Test]
        public void Goes17Himawari8()
        {
            // Simplified for ease of validation
            var goes17 = ToDefinition(140, -50, "GOES-17");
            var himawari8 = ToDefinition(60, -140, "Himawari-8");
            
            var calculator = new ProjectionOverlapCalculator();
            calculator.Initialise(new List<SatelliteDefinition> { goes17, himawari8 });
            
            VerifyRangeEquivalency(
                calculator.GetNonOverlappingRange(goes17),
                new Range(Angle.FromDegrees(180).Radians, Angle.FromDegrees(-50).Radians));
        }
        
        [Test]
        public void SingleSatellite()
        {
            var goes16 = ToDefinition(-156.2995, 6.2995);

            var calculator = new ProjectionOverlapCalculator();
            calculator.Initialise(new List<SatelliteDefinition> { goes16});

            VerifyRangeEquivalency(calculator.GetNonOverlappingRange(goes16), goes16.LongitudeRange);
        }

        [Test]
        public void NonOverlapping()
        {
            var goes16 = ToDefinition(-156.2995, 6.2995);
            var nonOverlapping = ToDefinition(60, 120);

            var calculator = new ProjectionOverlapCalculator();
            calculator.Initialise(new List<SatelliteDefinition> { goes16, nonOverlapping });

            VerifyRangeEquivalency(calculator.GetNonOverlappingRange(goes16), goes16.LongitudeRange);
            VerifyRangeEquivalency(calculator.GetNonOverlappingRange(nonOverlapping), nonOverlapping.LongitudeRange);
        }

        [Test]
        public void OverlappingSingleWithWrap()
        {
            var first = ToDefinition(-150, 10);
            var second = ToDefinition(140, -50);

            var calculator = new ProjectionOverlapCalculator();
            calculator.Initialise(new List<SatelliteDefinition> { first, second });

            VerifyRangeEquivalency(
                calculator.GetNonOverlappingRange(first),
                new Range(Angle.FromDegrees(-100), Angle.FromDegrees(10)));

            VerifyRangeEquivalency(
                calculator.GetNonOverlappingRange(second),
                new Range(Angle.FromDegrees(140), Angle.FromDegrees(260)));
        }

        [Test]
        public void OverlappingSingleNoWrap()
        {
            var first = ToDefinition(-100, 0);
            var second = ToDefinition(-50, 150);

            var calculator = new ProjectionOverlapCalculator();
            calculator.Initialise(new List<SatelliteDefinition> { first, second });

            VerifyRangeEquivalency(
                calculator.GetNonOverlappingRange(first),
                new Range(Angle.FromDegrees(-100), Angle.FromDegrees(-25)));

            VerifyRangeEquivalency(
                calculator.GetNonOverlappingRange(second),
                new Range(Angle.FromDegrees(-25), Angle.FromDegrees(150)));
        }

        [Test]
        public void OverlappingBoth()
        {
            // Overlapped by both second and third
            var first = ToDefinition(-150, 10);
            var second = ToDefinition(140, -50);
            var third = ToDefinition(0, 60);

            var calculator = new ProjectionOverlapCalculator();
            calculator.Initialise(new List<SatelliteDefinition> { first, second, third });

            VerifyRangeEquivalency(
                calculator.GetNonOverlappingRange(first),
                new Range(Angle.FromDegrees(-100), Angle.FromDegrees(5)));

            VerifyRangeEquivalency(
                calculator.GetNonOverlappingRange(second),
                new Range(Angle.FromDegrees(140), Angle.FromDegrees(260)));

            VerifyRangeEquivalency(
                calculator.GetNonOverlappingRange(third),
                new Range(Angle.FromDegrees(5), Angle.FromDegrees(60)));
        }


        private static SatelliteDefinition ToDefinition(double startDegrees, double endDegrees, string name = "")
        {
            var nonOverlapping = new SatelliteDefinition(name, "", FilenameParserType.Goesproc, 0,
                new Range(Angle.FromDegrees(-90), Angle.FromDegrees(90)),
                new Range(
                    Angle.FromDegrees(startDegrees),
                    Angle.FromDegrees(endDegrees)));
            
            return nonOverlapping;
        }
    }
}