namespace Sanchez.Processing.Services.Filesystem;

public interface IFilenameParser
{
    /// <summary>
    ///     Extracts the timestamp from a satellite imagery filename.
    /// </summary>
    DateTime? GetTimestamp(string filename);
}