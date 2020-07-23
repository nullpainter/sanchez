using System.Drawing;
using FluentAssertions;
using NUnit.Framework;
using Sanchez.Extensions;

namespace Sanchez.Test.Extensions
{
    [TestFixture(TestOf = typeof(TintExtensions))]
    public class TintExtensionsTests
    {
        [Test]
        public void TintMatrixTest()
        {
            var colour = Color.FromArgb(128, 255, 15);
            var tintMatrix = TintExtensions.CreateTintMatrix(colour);

            tintMatrix.M11.Should().Be(128f/255);
            tintMatrix.M22.Should().Be(255f/255);
            tintMatrix.M33.Should().Be(15f/255);
            tintMatrix.M44.Should().Be(1);

            // Verify that no other values set
            tintMatrix.M11 = tintMatrix.M22 = tintMatrix.M33 = 1;
            tintMatrix.IsIdentity.Should().BeTrue();
        }
    }
}