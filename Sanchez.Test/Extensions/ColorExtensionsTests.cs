using FluentAssertions;
using NUnit.Framework;
using Sanchez.Extensions;

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
            var colour = triplet.FromHexTriplet();
            Assert.NotNull(colour);

            colour.Value.R.Should().Be(170);
            colour.Value.G.Should().Be(187);
            colour.Value.B.Should().Be(204);
        }

        [Test]
        public void InvalidConversion()
        {
            var colour = "bananas".FromHexTriplet();
            Assert.Null(colour);
        }
    }
}