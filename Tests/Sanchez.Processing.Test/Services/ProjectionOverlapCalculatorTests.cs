using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Angles;
using Sanchez.Processing.Models.Configuration;
using Sanchez.Processing.Services;
using Sanchez.Processing.Services.Filesystem;
using Sanchez.Test.Common;

namespace Sanchez.Processing.Test.Services;

[TestFixture(TestOf = typeof(ProjectionOverlapCalculator))]
public class ProjectionOverlapCalculatorTests : AbstractTests
{
    private IProjectionOverlapCalculator Calculator => GetService<IProjectionOverlapCalculator>();

    [Test]
    public void Goes17Himawari8()
    {
        // Simplified for ease of validation
        var goes17 = ToDefinition(140, -50, "GOES-17");
        var himawari8 = ToDefinition(60, -140, "Himawari-8");

        Calculator.Initialise(new List<SatelliteDefinition> { goes17, himawari8 });

        VerifyRangeEquivalency(
            Calculator.GetNonOverlappingRange(goes17).Range,
            new AngleRange(Angle.FromDegrees(180).Radians, Angle.FromDegrees(310).Radians));
    }

    [Test]
    public void SingleSatellite()
    {
        var goes16 = ToDefinition(-156.2995, 6.2995);

        Calculator.Initialise(new List<SatelliteDefinition> { goes16 });

        VerifyRangeEquivalency(
            Calculator.GetNonOverlappingRange(goes16).Range,
            goes16.LongitudeRange);
    }

    [Test]
    public void NonOverlapping()
    {
        var goes16 = ToDefinition(-156.2995, 6.2995);
        var nonOverlapping = ToDefinition(60, 120);

        Calculator.Initialise(new List<SatelliteDefinition> { goes16, nonOverlapping });

        VerifyRangeEquivalency(
            Calculator.GetNonOverlappingRange(goes16).Range,
            goes16.LongitudeRange);

        VerifyRangeEquivalency(
            Calculator.GetNonOverlappingRange(nonOverlapping).Range,
            nonOverlapping.LongitudeRange);
    }

    [Test]
    public void OverlappingSingleWithWrap()
    {
        var first = ToDefinition(-150, 10);
        var second = ToDefinition(140, -50);

        Calculator.Initialise(new List<SatelliteDefinition> { first, second });

        VerifyRangeEquivalency(
            Calculator.GetNonOverlappingRange(first).Range,
            new AngleRange(Angle.FromDegrees(-100), Angle.FromDegrees(10)));

        VerifyRangeEquivalency(
            Calculator.GetNonOverlappingRange(second).Range,
            new AngleRange(Angle.FromDegrees(140), Angle.FromDegrees(260)));
    }

    [Test]
    public void OverlapRight()
    {
        var first = ToDefinition(-155, 5);
        var second = ToDefinition(-15, 140);

        Calculator.Initialise(new List<SatelliteDefinition> { second, first });

        VerifyRangeEquivalency(
            Calculator.GetNonOverlappingRange(first).Range,
            new AngleRange(Angle.FromDegrees(-155), Angle.FromDegrees(-5)));

        VerifyRangeEquivalency(
            Calculator.GetNonOverlappingRange(second).Range,
            new AngleRange(Angle.FromDegrees(-5), Angle.FromDegrees(140)));
    }

    [Test]
    public void OverlappingSingleNoWrap()
    {
        var first = ToDefinition(-100, 0);
        var second = ToDefinition(-50, 150);

        Calculator.Initialise(new List<SatelliteDefinition> { first, second });

        VerifyRangeEquivalency(
            Calculator.GetNonOverlappingRange(first).Range,
            new AngleRange(Angle.FromDegrees(-100), Angle.FromDegrees(-25)));

        VerifyRangeEquivalency(
            Calculator.GetNonOverlappingRange(second).Range,
            new AngleRange(Angle.FromDegrees(-25), Angle.FromDegrees(150)));
    }

    [Test]
    public void OverlappingBoth()
    {
        // Overlapped by both second and third
        var first = ToDefinition(-150, 10);
        var second = ToDefinition(140, -50);
        var third = ToDefinition(0, 60);

        Calculator.Initialise(new List<SatelliteDefinition> { first, second, third });

        VerifyRangeEquivalency(
            Calculator.GetNonOverlappingRange(first).Range,
            new AngleRange(Angle.FromDegrees(-100), Angle.FromDegrees(5)));

        VerifyRangeEquivalency(
            Calculator.GetNonOverlappingRange(second).Range,
            new AngleRange(Angle.FromDegrees(140), Angle.FromDegrees(260)));

        VerifyRangeEquivalency(
            Calculator.GetNonOverlappingRange(third).Range,
            new AngleRange(Angle.FromDegrees(5), Angle.FromDegrees(60)));
    }

    private static SatelliteDefinition ToDefinition(double startDegrees, double endDegrees, string name = "")
    {
        var nonOverlapping = new SatelliteDefinition(name, "", "", 
            FilenameParserType.Goesproc,
            false,
            0, new AngleRange(Angle.FromDegrees(-90), Angle.FromDegrees(90)),
            new AngleRange(
                Angle.FromDegrees(startDegrees),
                Angle.FromDegrees(endDegrees)));

        return nonOverlapping;
    }

    private static void VerifyRangeEquivalency(AngleRange first, AngleRange second)
    {
        Angle.FromRadians(first.Start).Degrees
            .Should().BeApproximately(Angle.FromRadians(second.Start).Degrees, Precision);

        Angle.FromRadians(first.End).Degrees
            .Should().BeApproximately(Angle.FromRadians(second.End).Degrees, Precision);
    }
}