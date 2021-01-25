using System;
using FluentAssertions;
using NUnit.Framework;
using Sanchez.Processing.Services.Filesystem;
using Sanchez.Processing.Services.Filesystem.Parsers;

namespace Sanchez.Processing.Test.Filesystem.FilenameParsers
{
    [TestFixture(TestOf = typeof(Gk2AFilenameParser))]
    public class Gk2AFilenameParserTests : AbstractFilenameParserTests
    {
        [Test]
        public void ExtractDate()
        {
            var definition = NewDefinition(FilenameParserType.Xrit);
            var date = definition.FilenameParser.GetTimestamp("IMG_FD_020_IR105_20190907_032006.jpg");
            date.Should().Be(new DateTime(2019, 09, 07, 03, 20, 06));
        }

        [Test]
        public void MissingDate()
        {
            var definition = NewDefinition(FilenameParserType.Xrit);
            var date = definition.FilenameParser.GetTimestamp("GOES17_FD_CH13_20200830T033031Z.jpg");
            date.Should().BeNull();
        }

        [Test]
        public void UnhandledSuffixNotParsed()
        {
            var definition = NewDefinition(FilenameParserType.Xrit);
            var date = definition.FilenameParser.GetTimestamp("IMG_FD_020_IR105_20190907_032006_ENHANCED.jpg");
            date.Should().BeNull();
        }
        
        [Test]
        public void SuffixParsed()
        {
            var definition = NewDefinition(FilenameParserType.Xrit, suffix: "_ENHANCED");
            var date = definition.FilenameParser.GetTimestamp("IMG_FD_020_IR105_20190907_032006_ENHANCED.jpg");
            date.Should().Be(new DateTime(2019, 09, 07, 03, 20, 06));
        }
    }
}