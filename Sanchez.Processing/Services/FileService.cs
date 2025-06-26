using DotNet.Globbing;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Projections;

namespace Sanchez.Processing.Services;

public interface IFileService
{
    /// <summary>
    ///     Returns a list of files to process, based on <see cref="RenderOptions.SourcePath"/>. This property
    ///     can be a single file, a directory or a glob and wildcard pattern (such as <c>source/**/*IR.jpg</c>)
    /// </summary>
    List<string> GetSourceFiles();

    List<Registration> ToRegistrations(List<string> sourceFiles, CancellationToken ct = default);

    /// <summary>
    ///     Returns whether the output file should be written, based on options and whether the file already exists.
    /// </summary>
    bool ShouldWrite(string path);
}

public class FileService(RenderOptions options, ISatelliteRegistry registry) : IFileService
{
    public List<Registration> ToRegistrations(List<string> sourceFiles, CancellationToken ct = default)
    {
        var registrations = new List<Registration>();

        foreach (var file in sourceFiles)
        {
            if (ct.IsCancellationRequested) return registrations;
                
            var (definition, timestamp) = registry.Locate(file);
            if (definition == null) continue;

            registrations.Add(new Registration(file, definition, timestamp));
        }

        return registrations;
    }

    /// <summary>
    ///     Returns a list of files to process, based on <see cref="RenderOptions.SourcePath"/>. This property
    ///     can be a single file, a directory or a glob and wildcard pattern (such as <c>source/**/*IR.jpg</c>)
    /// </summary>
    public List<string> GetSourceFiles()
    {
        var absolutePath = Path.GetFullPath(options.SourcePath);

        // Source is a single file
        if (!options.MultipleSources)
        {
            return File.Exists(absolutePath) ? [absolutePath] : [];
        }

        // If the source is a directory, enumerate all files
        if (Directory.Exists(absolutePath))
        {
            return Directory
                .GetFiles(absolutePath, "*.*", SearchOption.AllDirectories)
                .OrderBy(file => file)
                .ToList();
        }

        // Source is a glob, so enumerate all files in its base directory directory and return
        // glob matches
        var sourceGlob = Glob.Parse(absolutePath);

        return Directory
            .GetFiles(GetGlobBase(absolutePath), "*.*", SearchOption.AllDirectories)
            .Where(file => sourceGlob.IsMatch(file))
            .OrderBy(file => file)
            .ToList();
    }

    /// <summary>
    ///     Returns whether the output file should be written, based on options and whether the file already exists.
    /// </summary>
    public bool ShouldWrite(string path)
    {
        // Verify that the output file doesn't already exist and that the target folder isn't a file if using a bulk source
        return options.Force || !File.Exists(path);
    }

    private static string GetGlobBase(string path)
    {
        // Normalise separators
        path = path.Replace('\\', '/');

        // Extract all directories in the path prior to the glob pattern. Note that the glob library
        // also supports [a-z] style ranges, however we don't.
        var directorySegments = path
            .Split('/')
            .TakeWhile(segment => !segment.Contains('?') && !segment.Contains('*'))
            .ToList();

        // Recombine path
        return string.Join(Path.DirectorySeparatorChar, directorySegments);
    }
}