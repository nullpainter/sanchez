using NUnit.Framework;

namespace Funhouse.Test
{
    [TestFixture(TestOf = typeof(Funhouse))]
    public class EndToEndTests
    {
        [Test]
        // TODO make this a proper test plz
        public void Stitched()
        {
            Funhouse.Main( 
                "-o", "out.jpg", 
                "-s", "foo",
                "--stitch",
                "--autocrop");
         
            Assert.True(true, "Write me");
        }
    }
}