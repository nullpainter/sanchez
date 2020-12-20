using System;
using FluentAssertions;
using NUnit.Framework;
using Sanchez.Processing.Services.Filesystem;
using Sanchez.Processing.Services.Filesystem.Parsers;

namespace Sanchez.Processing.Test.Filesystem
{
    [TestFixture(TestOf = typeof(Gk2AFilenameParser))]
    public class Gk2AFilenameParserTests : AbstractFilenameParserTests
    {
        private readonly IFilenameParser _filenameParser = new Gk2AFilenameParser();

        [Test]
        public void ExtractDate()
        {
            var date = _filenameParser.GetTimestamp("IMG_FD_020_IR105_20190907_032006.jpg", NewDefinition());
            date.Should().Be(new DateTime(2019, 09, 07, 03, 20, 06));
        }

        [Test]
        public void MissingDate()
        {
            var date = _filenameParser.GetTimestamp("GOES17_FD_CH13_20200830T033031Z.jpg", NewDefinition());
            date.Should().BeNull();
        }

        [Test]
        public void Suffix()
        {
            var date = _filenameParser.GetTimestamp("IMG_FD_020_IR105_20190907_032006_ENHANCED.jpg", NewDefinition());
            date.Should().BeNull();
        }
    }
}