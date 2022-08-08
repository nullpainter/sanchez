using Sanchez.Processing.Filesystem;
using Sanchez.Processing.Models;
using Sanchez.Test.Common;

namespace Sanchez.Processing.Test.Filesystem;

[TestFixture(TestOf = typeof(SingleFilenameProvider))]
public class SingleFilenameProviderTests : AbstractTests
{
    private SingleFilenameProvider FilenameProvider => GetService<SingleFilenameProvider>();
        
    [Test]
    public void OutputFilenameNonBatch()
    {
        RenderOptions.SourcePath = "source.jpg";
        RenderOptions.OutputPath = Path.Combine("test", "output.jpg");
        RenderOptions.OutputFormat = ImageFormats.Png;

        var outputFilename = FilenameProvider.GetOutputFilename("source.jpg");
        outputFilename.Should().Be(Path.Combine("test", "output.png"));
    }
        
    [Test]
    public void OutputFilenameNonBatchDirectory()
    {
        RenderOptions.SourcePath = "source.jpg";
        RenderOptions.OutputPath = "test";
        RenderOptions.OutputFormat = ImageFormats.Jpeg;

        var outputFilename = FilenameProvider.GetOutputFilename("source.jpg");
        outputFilename.Should().Be(Path.Combine("test", "source-FC.jpg"));
    }
        
    [Test]
    public void OutputFilenameBatch()
    {
        RenderOptions.SourcePath = "images/**";
        RenderOptions.OutputPath = Path.Combine("test", "output");

        var outputFilename = FilenameProvider.GetOutputFilename("source.jpg");
        outputFilename.Should().Be(Path.Combine("test", "output", "source-FC.jpg"));
    }
}