using NUnit.Framework;
using Sanchez.Processing.Extensions;
using Sanchez.Processing.Models;
using Sanchez.Test.Common;

namespace Sanchez.Processing.Test.Extensions
{
    [TestFixture(TestOf = typeof(AngleExtensions))]
    public class AngleExtensionsTests : AbstractTests
    {
        [TestCase(0, 200, ExpectedResult = 100)]
        [TestCase(-180, 200, ExpectedResult = 0)]
        [TestCase(180, 200, ExpectedResult = 200)]
        public int ScaleToWidth(double value, int width) => Angle.FromDegrees(value).Radians.ScaleToWidth(width);
        
        [TestCase(0, 200, ExpectedResult = 100)]
        [TestCase(-90, 200, ExpectedResult = 200)]
        [TestCase(90, 200, ExpectedResult = 0)]
        public int ScaleToHeight(double value, int width) => Angle.FromDegrees(value).Radians.ScaleToHeight(width);
    }
}