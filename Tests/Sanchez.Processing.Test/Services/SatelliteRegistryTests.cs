using System;
using FluentAssertions;
using NUnit.Framework;
using Sanchez.Processing.Services.Filesystem;
using Sanchez.Test.Common;

namespace Sanchez.Processing.Test.Services
{
    [TestFixture]
    public class SatelliteRegistryTests : AbstractTests
    {
        [Test]
        [Timeout(1000)]
        public void PerformanceTest()
        {
            for (var i = 0; i < 10000; i++)
            {
                var (definition, _) = SatelliteRegistry.Locate("c:/images/GOES17_FD_CH13_20200911T080031Z.jpg");
                definition.Should().NotBeNull();
                
                (definition, _) = SatelliteRegistry.Locate("c:/images/IMG_FD_020_IR105_20190907_032006.jpg");
                definition.Should().NotBeNull(); 

                (definition, _) = SatelliteRegistry.Locate("c:/images/EWS-G1_20200911T080031Z.jpg");
                definition.Should().NotBeNull();
            }
        }

        [Test]
        public void Goes17Matched()
        {
            var (definition, timestamp) = SatelliteRegistry.Locate("c:/images/GOES17_FD_CH13_20200911T080031Z.jpg");
            definition.Should().NotBeNull("satellite definition should have been found");
            timestamp.Should().NotBeNull("timestamp should have been extracted");

            definition!.FilenameParserType.Should().Be(FilenameParserType.Goesproc);
            timestamp.Should().Be(new DateTime(2020, 09, 11, 8, 0, 31));
        }

        [Test]
        public void Goes16EnhancedNotMatched()
        {
            var (definition, _) = SatelliteRegistry.Locate("c:/images/GOES16_FD_CH13_enhanced_20200908T005019Z.jpg");
            definition.Should().BeNull("satellite definition should have not have been found for enhanced images");
        }

        [Test]
        public void Goes16Matched()
        {
            var (definition, timestamp) = SatelliteRegistry.Locate("c:/images/GOES16_FD_CH13_20200908T005019Z.jpg");
            definition.Should().NotBeNull("satellite definition should have been found");
            timestamp.Should().NotBeNull("timestamp should have been extracted");

            definition!.FilenameParserType.Should().Be(FilenameParserType.Goesproc);
            timestamp.Should().Be(new DateTime(2020, 09, 08, 0, 50, 19));
        }

        [Test]
        public void Gk2AMatched()
        {
            var (definition, timestamp) = SatelliteRegistry.Locate("c:/images/IMG_FD_003_IR105_20201217_003006.jpg");
            definition.Should().NotBeNull("satellite definition should have been found");
            timestamp.Should().NotBeNull("timestamp should have been extracted");

            definition!.FilenameParserType.Should().Be(FilenameParserType.Xrit);
            timestamp.Should().Be(new DateTime(2020, 12, 17, 00, 30, 06));
        }

        [Test]
        public void HimawariMatched()
        {
            var (definition, timestamp) = SatelliteRegistry.Locate("c:/images/Himawari8_FD_IR_20200908T015100Z.jpg");
            definition.Should().NotBeNull("satellite definition should have been found");
            timestamp.Should().NotBeNull("timestamp should have been extracted");

            definition!.FilenameParserType.Should().Be(FilenameParserType.Goesproc);
            timestamp.Should().Be(new DateTime(2020, 09, 08, 01, 51, 0));
        }

        [Test]
        public void ElectroMatched()
        {
            // Verify 4-9 definition
            var (definition, timestamp) = SatelliteRegistry.Locate("c:/images/200830_0230_6.jpg");
            definition.Should().NotBeNull("satellite definition should have been found");
            timestamp.Should().NotBeNull("timestamp should have been extracted");

            definition!.FilenameParserType.Should().Be(FilenameParserType.Electro);
            timestamp.Should().Be(new DateTime(2020, 08, 29, 23, 30, 0));

            // Verify 1-3 definition
            (definition, _) = SatelliteRegistry.Locate("c:/images/200830_0230_1.jpg");
            definition.Should().NotBeNull("satellite definition should have been found");
        }

        [Test]
        public void NwsNotMatched()
        {
            var (definition, _) = SatelliteRegistry.Locate("c:/images/20210101T230000Z_20210101230001-pac48per_latestBW.gif");
            definition.Should().BeNull("satellite definition should not have been found for an NWS image");
        }
    }
}