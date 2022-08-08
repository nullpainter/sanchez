using Sanchez.Processing.Models.Configuration;
using Sanchez.Processing.Services.Filesystem;
using Sanchez.Processing.Services.Filesystem.Parsers;

namespace Sanchez.Processing.Test.Filesystem.FilenameParsers;

[TestFixture(TestOf = typeof(GoesFilenameParser))]
public class GoesFilenameParserTests : AbstractFilenameParserTests
{
    private readonly SatelliteDefinition _definition = NewDefinition(FilenameParserType.Goesproc, "GOES16_FD_CH13_");

    [Test]
    public void CustomSuffix()
    {
        var definition = NewDefinition(FilenameParserType.Goesproc, "GOES16_FD_CH13_", "_ABC");
        var date = definition.FilenameParser.GetTimestamp("GOES16_FD_CH13_20200830T035020Z_ABC.jpg");
        date.Should().Be(new DateTime(2020, 08, 30, 03, 50, 20));
    }

    [Test]
    public void ValidPrefix()
    {
        var date = _definition.FilenameParser.GetTimestamp("GOES16_FD_CH13_20200830T035020Z.jpg");
        date.Should().Be(new DateTime(2020, 08, 30, 03, 50, 20));
    }

    [Test]
    public void InvalidPrefix()
    {
        var date = _definition.FilenameParser.GetTimestamp("FOO_FD_CH13_20200830T035020Z.jpg");
        date.Should().BeNull("date shouldn't be extracted with invalid prefix");
    }

    [Test]
    public void ExtractDate()
    {
        var date = _definition.FilenameParser.GetTimestamp("GOES16_FD_CH13_20200830T035020Z.jpg");
        date.Should().Be(new DateTime(2020, 08, 30, 03, 50, 20));

        var goes17Definition = NewDefinition(FilenameParserType.Goesproc, "GOES17_FD_CH13_");
        date = goes17Definition.FilenameParser.GetTimestamp("GOES17_FD_CH13_20200830T033031Z.jpg");
        date.Should().Be(new DateTime(2020, 08, 30, 03, 30, 31));

        var himawari8Definition = NewDefinition(FilenameParserType.Goesproc, "Himawari8_FD_IR_");
        date = himawari8Definition.FilenameParser.GetTimestamp("Himawari8_FD_IR_20200830T035100Z.jpg");
        date.Should().Be(new DateTime(2020, 08, 30, 03, 51, 00));
    }

    [Test]
    public void MissingDate()
    {
        var date = _definition.FilenameParser.GetTimestamp("IMG_FD_020_IR105_20190907_032006.jpg");
        date.Should().BeNull();
    }

    [Test]
    public void Suffix()
    {
        var date = _definition.FilenameParser.GetTimestamp("GOES16_FD_CH13_20200830T035020Z-FC.jpg");
        date.Should().BeNull();
    }
}