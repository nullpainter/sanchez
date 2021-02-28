using System;
using FluentAssertions;
using NUnit.Framework;
using Sanchez.Processing.Services.Filesystem;
using Sanchez.Processing.Services.Filesystem.Parsers;

namespace Sanchez.Processing.Test.Filesystem.FilenameParsers
{
    [TestFixture(TestOf = typeof(ElectroFilenameParser))]
    public class ElectroFilenameParserTests : AbstractFilenameParserTests
    {
        [Test]
        public void ExtractDatePrefix()
        {
            var definition = NewDefinition(FilenameParserType.Electro, "test_", "4");
            var date = definition.FilenameParser.GetTimestamp("test_200920_1730_4.jpg");
            date.Should().Be(new DateTime(2020, 09, 20, 14, 30, 00));
        }

        [TestCase("4")]
        [TestCase("[1-4]")]
        public void ExtractDateMatchingSuffix(string suffix)
        {
            var definition = NewDefinition(FilenameParserType.Electro, suffix: suffix);
            var date = definition.FilenameParser.GetTimestamp("200920_1730_4.jpg");
            date.Should().Be(new DateTime(2020, 09, 20, 14, 30, 00));
        }

        [TestCase("3")]
        [TestCase("[1-3]")]
        public void ExtractDateNotMatchingSuffix(string suffix)
        {
            var definition = NewDefinition(FilenameParserType.Electro, suffix: suffix);
            var date = definition.FilenameParser.GetTimestamp("200920_1730_4.jpg"); 
            date.Should().BeNull();
        }

        [Test]
        public void MissingDate()
        {
            var definition = NewDefinition(FilenameParserType.Electro);
            var date = definition.FilenameParser.GetTimestamp("2020.jpg");
            date.Should().BeNull();
        }
    }
}