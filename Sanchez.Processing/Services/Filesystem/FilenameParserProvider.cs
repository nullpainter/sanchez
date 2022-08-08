using Sanchez.Processing.Services.Filesystem.Parsers;

namespace Sanchez.Processing.Services.Filesystem;

public static class FilenameParserProvider
{
    public static IFilenameParser GetParser(FilenameParserType type, string? prefix, string? suffix)
    {
        return type switch
        {
            FilenameParserType.Goesproc => new GoesFilenameParser(prefix, suffix),
            FilenameParserType.Xrit => new Gk2AFilenameParser(prefix, suffix),
            FilenameParserType.Electro => new ElectroFilenameParser(prefix, suffix),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}