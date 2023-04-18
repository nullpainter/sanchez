using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Angles;
using Sanchez.Processing.Models.Projections;
using Sanchez.Test.Common;
using Sanchez.Workflow.Steps.Equirectangular;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Test.Equirectangular;

[TestFixture(TestOf = typeof(GetCropBoundsTests))]
public class GetCropBoundsTests : AbstractTests
{
    private GetCropBounds _step;
    private RenderOptions Options => GetService<RenderOptions>();

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        _step = GetService<GetCropBounds>();
    }

    [Test]
    public async Task ExplicitCropFull()
    {
        using var image = new Image<Rgba32>(100, 100);
        _step.TargetImage = image;
        _step.Activity = new Activity(Array.Empty<Registration>());

        var latitudeRange = new AngleRange(Angle.FromDegrees(90), Angle.FromDegrees(-90));
        var longitudeRange = new AngleRange(Angle.FromDegrees(-180), Angle.FromDegrees(180));
        Options.EquirectangularRender = new EquirectangularRenderOptions(false, false, false, latitudeRange, longitudeRange);

        // Run method under test
        await _step.RunAsync(new StepExecutionContext());

        Assert.NotNull(_step.CropBounds);
        _step.CropBounds.Should().BeEquivalentTo(new Rectangle(-50, 0, 100, 100));
    }

    [Test]
    public async Task ExplicitCropWrap()
    {
        using var image = new Image<Rgba32>(100, 100);
        _step.TargetImage = image;
        _step.Activity = new Activity(Array.Empty<Registration>());

        var longitudeRange = new AngleRange(Angle.FromDegrees(0), Angle.FromDegrees(360));
        Options.EquirectangularRender = new EquirectangularRenderOptions(false, false, false, null, longitudeRange);

        // Run method under test
        await _step.RunAsync(new StepExecutionContext());

        Assert.NotNull(_step.CropBounds);
        _step.CropBounds.Should().BeEquivalentTo(new Rectangle(0, 0, 100, 100));
    }


    [Test]
    public async Task ExplicitCropPartial()
    {
        using var image = new Image<Rgba32>(100, 100);

        _step.TargetImage = image;
        _step.Activity = new Activity(Array.Empty<Registration>());

        var latitudeRange = new AngleRange(Angle.FromDegrees(0), Angle.FromDegrees(90));
        var longitudeRange = new AngleRange(Angle.FromDegrees(0), Angle.FromDegrees(180));
        Options.EquirectangularRender = new EquirectangularRenderOptions(false, false, false, latitudeRange, longitudeRange);

        // Run method under test
        await _step.RunAsync(new StepExecutionContext());

        Assert.NotNull(_step.CropBounds);
        _step.CropBounds.Should().BeEquivalentTo(new Rectangle(0, 0, 50, 50));
    }
}