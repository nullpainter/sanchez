using System;
using FluentAssertions;
using Funhouse.Services.Filesystem;
using NUnit.Framework;

namespace Funhouse.Test.Filesystem
{
    [TestFixture]
    public class GoesFilenameParserTests : AbstractTests
    {
        private IFilenameParser FilenameParser = new GoesFilenameParser();

        [Test]
        public void ExtractDate()
        {
            var date = FilenameParser.GetTimestamp("GOES16_FD_CH13_20200830T035020Z.jpg");
            date.Should().Be(new DateTime(2020, 08, 30, 03, 50, 20).ToLocalTime());

            date = FilenameParser.GetTimestamp("GOES17_FD_CH13_20200830T033031Z.jpg");
            date.Should().Be(new DateTime(2020, 08, 30, 03, 30, 31).ToLocalTime());

            date = FilenameParser.GetTimestamp("Himawari8_FD_IR_20200830T035100Z.jpg");
            date.Should().Be(new DateTime(2020, 08, 30, 03, 51, 00).ToLocalTime());
        }

        [Test]
        public void MissingDate()
        {
            var date = FilenameParser.GetTimestamp("IMG_FD_020_IR105_20190907_032006.jpg");
            date.Should().BeNull();
        }

        [Test]
        public void Suffix()
        {
            var date = FilenameParser.GetTimestamp("GOES16_FD_CH13_20200830T035020Z-FC.jpg");
            date.Should().BeNull();
        }
    }
}