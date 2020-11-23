using System;
using FluentAssertions;
using NUnit.Framework;
using Sanchez.Processing.Services.Filesystem;
using Sanchez.Processing.Services.Filesystem.Parsers;
using Sanchez.Test.Common;

namespace Sanchez.Processing.Test.Filesystem
{
    [TestFixture(TestOf = typeof(GoesFilenameParser))]
    public class GoesFilenameParserTests : AbstractFilenameParserTests
    {
        private readonly IFilenameParser _filenameParser = new GoesFilenameParser();

        [Test]
        public void ValidPrefix()
        {
            var definition = NewDefinition("GOES16");
            var date = _filenameParser.GetTimestamp("GOES16_FD_CH13_20200830T035020Z.jpg", definition);
            date.Should().Be(new DateTime(2020, 08, 30, 03, 50, 20));
        }
        
        [Test]
        public void InvalidPrefix()
        {
            var definition = NewDefinition("GOES16");
            var date = _filenameParser.GetTimestamp("FOO_FD_CH13_20200830T035020Z.jpg", definition);
            date.Should().BeNull("date shouldn't be extracted with invalid prefix");
        }

        [Test]
        public void ExtractDate()
        {
            var date = _filenameParser.GetTimestamp("GOES16_FD_CH13_20200830T035020Z.jpg", NewDefinition());
            date.Should().Be(new DateTime(2020, 08, 30, 03, 50, 20));

            date = _filenameParser.GetTimestamp("GOES17_FD_CH13_20200830T033031Z.jpg", NewDefinition());
            date.Should().Be(new DateTime(2020, 08, 30, 03, 30, 31));

            date = _filenameParser.GetTimestamp("Himawari8_FD_IR_20200830T035100Z.jpg", NewDefinition());
            date.Should().Be(new DateTime(2020, 08, 30, 03, 51, 00));
        }

        [Test]
        public void MissingDate()
        {
            var date = _filenameParser.GetTimestamp("IMG_FD_020_IR105_20190907_032006.jpg", NewDefinition());
            date.Should().BeNull();
        }

        [Test]
        public void Suffix()
        {
            var date = _filenameParser.GetTimestamp("GOES16_FD_CH13_20200830T035020Z-FC.jpg", NewDefinition());
            date.Should().BeNull();
        }
    }
}