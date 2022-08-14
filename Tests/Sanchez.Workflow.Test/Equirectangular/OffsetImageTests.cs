using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Angles;
using Sanchez.Processing.Models.Configuration;
using Sanchez.Processing.Models.Projections;
using Sanchez.Processing.Services.Filesystem;
using Sanchez.Test.Common;
using Sanchez.Workflow.Steps.Equirectangular;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Test.Equirectangular;

[TestFixture(TestOf = typeof(OffsetImage))]
public class OffsetImageTests : AbstractTests
{
    private OffsetImage _step;
    private RenderOptions Options => GetService<RenderOptions>();
    private Image<Rgba32> _image;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        _step = GetService<OffsetImage>();

        // Create a sample image, filling a region so we can verify offset
        _image = new Image<Rgba32>(100, 100);
        _image.Mutate(c => c.BackgroundColor(Color.Red, new Rectangle(0, 0, 50, 50)));
    }

    public override void TearDown()
    {
        base.TearDown();
        _image.Dispose();
    }

    // public async Task NoOffsetForFullCoverage()
    // {
    //      Assert.Fail("write me");           
    // }
    //
    // public async Task OffsetForFullCoverageWithCrop()
    // {
    //     Assert.Fail("write me");
    // }

    [Test]
    public async Task OffsetImage()
    {
        Options.EquirectangularRender = new EquirectangularRenderOptions(false, false, false);

        _step.TargetImage = _image;
        _step.GlobalOffset = 0;    // 180 degrees

        var registration = new Registration
            ("", new SatelliteDefinition("", null, null, FilenameParserType.Xrit, false, 0, new AngleRange(0, 0), new AngleRange(0, 0)), null)
            {
                LongitudeRange = new ProjectionRange(new AngleRange(0, 0))
            };

        _step.Activity = new Activity(new[] { registration });

        // Source image sanity check
        _image[0, 0].R.Should().Be(255);
        _image[99, 0].R.Should().Be(0);
        _image[0, 99].R.Should().Be(0);
        _image[99, 99].R.Should().Be(0);
        _image[49, 0].R.Should().Be(255);
        _image[50, 0].R.Should().Be(0);

        // Run method under test
        await _step.RunAsync(new StepExecutionContext());
            
        // Verify red square has been offset
        _image[0, 0].R.Should().Be(0);
        _image[99, 0].R.Should().Be(255);
        _image[0, 99].R.Should().Be(0);
        _image[99, 99].R.Should().Be(0);
        _image[49, 0].R.Should().Be(0);
        _image[50, 0].R.Should().Be(255);
    }
}