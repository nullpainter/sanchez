using Sanchez.Processing.Filesystem.Equirectangular;
using Sanchez.Processing.Models;
using Sanchez.Test.Common;

namespace Sanchez.Processing.Test.Filesystem.Equirectangular;

[TestFixture(TestOf = typeof(StitchedFilenameProvider))]
public class StitchedFilenameProviderTests : AbstractTests
{
    private StitchedFilenameProvider FilenameProvider => GetService<StitchedFilenameProvider>();

    [Test]
    public void PathHasExtension_Png()
    {
        RenderOptions.OutputFormat = ImageFormats.Png;
        RenderOptions.OutputPath = "/tmp/foo.jpg";

        FilenameProvider.GetOutputFilename(DateTime.Now).Should().Be("/tmp/foo.png");
    }

    [Test]
    public void PathHasExtension_Jpg()
    {
        RenderOptions.OutputFormat = ImageFormats.Jpeg;
        RenderOptions.OutputPath = "/tmp/foo.jpg";

        FilenameProvider.GetOutputFilename(DateTime.Now).Should().Be("/tmp/foo.jpg");
    }

    [Test]
    public void PathHasNoExtension()
    {
        RenderOptions.OutputFormat = ImageFormats.Png;
        RenderOptions.OutputPath = "/tmp/images";

        FilenameProvider
            .GetOutputFilename(new DateTime(2021, 12, 20, 14, 00, 30))
            .Replace("\\", "/")
            .Should().Be("/tmp/images/stitched-20211220T140030Z.png");
    }
}