using System;
using FluentAssertions;
using NUnit.Framework;
using Sanchez.Processing.Services.Filesystem;
using Sanchez.Processing.Services.Filesystem.Parsers;
using Sanchez.Test.Common;

namespace Sanchez.Processing.Test.Filesystem
{
    [TestFixture(TestOf = typeof(GoesFilenameParser))]
    public class GoesFilenameParserTests : AbstractTests
    {
        private readonly IFilenameParser _filenameParser = new GoesFilenameParser();

        [Test]
        public void ExtractDate()
        {
            var date = _filenameParser.GetTimestamp("GOES16_FD_CH13_20200830T035020Z.jpg");
            date.Should().Be(new DateTime(2020, 08, 30, 03, 50, 20));

            date = _filenameParser.GetTimestamp("GOES17_FD_CH13_20200830T033031Z.jpg");
            date.Should().Be(new DateTime(2020, 08, 30, 03, 30, 31));

            date = _filenameParser.GetTimestamp("Himawari8_FD_IR_20200830T035100Z.jpg");
            date.Should().Be(new DateTime(2020, 08, 30, 03, 51, 00));
        }

        [Test]
        public void MissingDate()
        {
            var date = _filenameParser.GetTimestamp("IMG_FD_020_IR105_20190907_032006.jpg");
            date.Should().BeNull();
        }

        [Test]
        public void Suffix()
        {
            var date = _filenameParser.GetTimestamp("GOES16_FD_CH13_20200830T035020Z-FC.jpg");
            date.Should().BeNull();
        }
    }
}