using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Sanchez.Processing.Extensions;
using Sanchez.Test.Common;

namespace Sanchez.Processing.Test.Extensions
{
    [TestFixture(TestOf = typeof(MathExtensions))]
    public class MathExtensionsTests : AbstractTests
    {
        [TestCase(0, 0)]
        [TestCase(1.999, 1.999)]
        [TestCase(2.5, -1.5)]
        [TestCase(4, 0)]
        [TestCase(-3.5, 0.5)]
        [TestCase(-3, 1)]
        public void Limit(double value, double expected)
        {
            value.Limit(-2, 2).Should().BeApproximately(expected, Precision);
        }

        [Test]
        public void ClosestTo()
        {
            var values = new[] { 1, 2, 2.5, 3 }.ToList();

            values.ClosestTo(1.2).Should().Be(1);
            values.ClosestTo(2.4).Should().Be(2.5);
            values.ClosestTo(2.5).Should().Be(2.5);
        }
    }
}