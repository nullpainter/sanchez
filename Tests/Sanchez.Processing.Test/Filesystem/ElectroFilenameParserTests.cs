using System;
using System.Diagnostics;
using FluentAssertions;
using NUnit.Framework;
using Sanchez.Processing.Services.Filesystem;
using Sanchez.Processing.Services.Filesystem.Parsers;

namespace Sanchez.Processing.Test.Filesystem
{
    [TestFixture(TestOf = typeof(ElectroFilenameParser))]
    public class ElectroFilenameParserTests : AbstractFilenameParserTests
    {
        private readonly IFilenameParser _filenameParser = new ElectroFilenameParser();

        [Test]
        public void ExtractDateNoSuffix()
        {
            var date = _filenameParser.GetTimestamp("200920_1730_4.jpg", NewDefinition());
            date.Should().Be(new DateTime(2020, 09, 20, 17, 30, 00));
        }

        [TestCase("4")]
        [TestCase("[1-4]")]
        public void ExtractDateMatchingSuffix(string suffix)
        {
            var date = _filenameParser.GetTimestamp("200920_1730_4.jpg", NewDefinition(suffix: suffix));
            date.Should().Be(new DateTime(2020, 09, 20, 17, 30, 00));
        }

        [TestCase("3")]
        [TestCase("[1-3]")]
        public void ExtractDateNotMatchingSuffix(string suffix)
        {
            var date = _filenameParser.GetTimestamp("200920_1730_4.jpg", NewDefinition(suffix: suffix));
            date.Should().BeNull();
        }

        [Test]
        public void MissingDate()
        {
            var date = _filenameParser.GetTimestamp("2020.jpg", NewDefinition());
            date.Should().BeNull();
        }
    }
}