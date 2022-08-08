using Sanchez.Processing.Models;

namespace Sanchez.Processing.Filesystem;

public class SingleFilenameProvider : IFilenameProvider
{
    private readonly RenderOptions _options;

    public SingleFilenameProvider(RenderOptions options) => _options = options;

    public string GetOutputFilename(string? input = null)
    {
        ArgumentNullException.ThrowIfNull(input);
        var targetExtension = _options.GetTargetExtension();
            
        var outputPath = _options.OutputPath;
        if (Path.GetExtension(outputPath) != "" && !_options.MultipleTargets) return Path.ChangeExtension(outputPath, targetExtension);

        var outputFilename = $"{Path.GetFileNameWithoutExtension(input)}{Constants.OutputFileSuffix}.{targetExtension}";
        return Path.Combine(outputPath, outputFilename);
    }

}