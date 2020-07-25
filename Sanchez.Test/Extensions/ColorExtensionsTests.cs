using FluentAssertions;
using NUnit.Framework;
using Sanchez.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Sanchez.Test.Extensions
{
    [TestFixture(TestOf = typeof(ColorExtensions))]
    public class ColorExtensionsTests
    {
        [TestCase("AABBCC")]
        [TestCase("#AABBCC")]
        [TestCase("#aabbcc")]
        public void TripletConversion(string triplet)
        {
            Color? colour = triplet.FromHexTriplet();
            Assert.NotNull(colour);
            Rgb24 rgb = colour.Value.ToPixel<Rgb24>();

            rgb.R.Should().Be(170);
            rgb.G.Should().Be(187);
            rgb.B.Should().Be(204);
        }

        [Test]
        public void InvalidConversion()
        {
            var colour = "bananas".FromHexTriplet();
            Assert.Null(colour);
        }
    }
}