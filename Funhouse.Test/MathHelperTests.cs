using FluentAssertions;
using Funhouse.Extensions;
using NUnit.Framework;

namespace Funhouse.Test
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
    }
}