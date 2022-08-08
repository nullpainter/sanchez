using Extend;
using Sanchez.Processing.Services;
using Sanchez.Test.Common;

namespace Sanchez.Processing.Test.Filesystem;

[TestFixture(TestOf = typeof(FileService))]
public class FileServiceTests : AbstractTests
{
    private IFileService FileService => GetService<IFileService>();

    [Test]
    public void GetSourceFiles()
    {
        var imageFolder = State.CreateTempDirectory();
        var sourceFile = Path.Combine(imageFolder, "source.jpg");
        var outputPath = Path.Combine(imageFolder, "output", "output.jpg");

        File.WriteAllText(sourceFile, "Hey!");

        RenderOptions.SourcePath = sourceFile;
        RenderOptions.OutputPath = outputPath;

        // Run method under test
        FileService.GetSourceFiles().Should().BeEquivalentTo(sourceFile);
    }

#if OS_WINDOWS
    private const string FirstPath = "source\\first\\2020\\firstImage.jpg";
    private const string SecondPath = "source\\first\\2020\\secondImage.png";
    private const string ThirdPath = "source\\second\\thirdImage.png";
#else
        private const string FirstPath = "source/first/2020/firstImage.jpg";
        private const string SecondPath = "source/first/2020/secondImage.png";
        private const string ThirdPath = "source/second/thirdImage.png";
#endif

    [TestCase("**/thirdImage.pn?", ExpectedResult = new[] { ThirdPath })]
    [TestCase("source/", ExpectedResult = new[] { FirstPath, SecondPath, ThirdPath })]
    [TestCase("source", ExpectedResult = new[] { FirstPath, SecondPath, ThirdPath })]
    [TestCase("source/**/*.*", ExpectedResult = new[] { FirstPath, SecondPath, ThirdPath })]
    [TestCase("source/f*/**", ExpectedResult = new[] { FirstPath, SecondPath })]
    [TestCase("source/**/*.png", ExpectedResult = new[] { SecondPath, ThirdPath })]
    [TestCase("source/first/2020/*.*", ExpectedResult = new[] { FirstPath, SecondPath })]
    [TestCase("source/**/2*/*.*", ExpectedResult = new[] { FirstPath, SecondPath })]
    public string[] GetSourceFilesBatch(string sourcePath)
    {
        // Prepare source files
        var imageFolder = State.CreateTempDirectory();
        var outputPath = Path.Combine(imageFolder, "output", "output.jpg");

        Directory.CreateDirectory(Path.Combine(imageFolder, "source", "first", "2020"));
        Directory.CreateDirectory(Path.Combine(imageFolder, "source", "second"));

        var files = new[]
        {
            Path.Combine(imageFolder, "source", "first", "2020", "firstImage.jpg"),
            Path.Combine(imageFolder, "source", "first", "2020", "secondImage.png"),
            Path.Combine(imageFolder, "source", "second", "thirdImage.png")
        };

        files.ForEach(path => File.WriteAllText(path, "Isn't this fun?"));

        RenderOptions.SourcePath = Path.Combine(imageFolder, sourcePath);
        RenderOptions.OutputPath = outputPath;

        // Run method under test
        return FileService
            .GetSourceFiles()
            .OrderBy(f => f)
            .Select(file => Path.GetRelativePath(imageFolder, file))
            .ToArray();
    }
}