using System;
using FluentAssertions;
using Funhouse.Services.Filesystem;
using Funhouse.Services.Filesystem.Parsers;
using NUnit.Framework;

namespace Funhouse.Test.Filesystem
{
    [TestFixture(TestOf = typeof(Gk2AFilenameParser))]
    public class Gk2AFilenameParserTests : AbstractTests
    {
        private IFilenameParser FilenameParser = new Gk2AFilenameParser();

        [Test]
        public void ExtractDate()
        {
            var date = FilenameParser.GetTimestamp("IMG_FD_020_IR105_20190907_032006.jpg");
            date.Should().Be(new DateTime(2019, 09, 07, 03, 20, 06));

        }

        [Test]
        public void MissingDate()
        {
            var date = FilenameParser.GetTimestamp("GOES17_FD_CH13_20200830T033031Z.jpg");
            date.Should().BeNull();
        }

        [Test]
        public void Suffix()
        {
            var date = FilenameParser.GetTimestamp("IMG_FD_020_IR105_20190907_032006_ENHANCED.jpg");
            date.Should().BeNull();
        }
    }
}