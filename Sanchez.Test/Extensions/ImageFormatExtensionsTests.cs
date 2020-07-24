using NUnit.Framework;
using Sanchez.Extensions;
using Sanchez.Models;

namespace Sanchez.Test.Extensions
{
    [TestFixture]
    public class ImageFormatExtensionsTests
    {
        [TestCase("pinky.jpg", ExpectedResult = ImageFormat.Jpeg)]
        [TestCase("pinky.JPEG", ExpectedResult = ImageFormat.Jpeg)]
        [TestCase("brain.png", ExpectedResult = ImageFormat.Png)]
        [TestCase("brain", ExpectedResult = null)]
        [TestCase("brain.bogus", ExpectedResult = null)]
        public ImageFormat? VerifyImageFormat(string filename) => filename.GetImageFormat();
    }
}