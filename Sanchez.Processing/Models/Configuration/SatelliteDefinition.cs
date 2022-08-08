using Sanchez.Processing.Extensions;
using Sanchez.Processing.Models.Angles;
using Sanchez.Processing.Services.Filesystem;

namespace Sanchez.Processing.Models.Configuration;

public class SatelliteDefinition
{
    /// <param name="displayName"></param>
    /// <param name="filenamePrefix"></param>
    /// <param name="filenameSuffix"></param>
    /// <param name="filenameParserType"></param>
    /// <param name="invert"></param>
    /// <param name="longitude"></param>
    /// <param name="latitudeRange"></param>
    /// <param name="longitudeRange"></param>
    /// <param name="height">Satellite height above Earth (metres)</param>
    /// <param name="crop"></param>
    /// <param name="brightness"></param>
    public SatelliteDefinition(
        string displayName,
        string? filenamePrefix,
        string? filenameSuffix,
        FilenameParserType filenameParserType,
        bool invert,
        double longitude,
        AngleRange latitudeRange,
        AngleRange longitudeRange,
        double height = Constants.Satellite.DefaultHeight,
        double[]? crop = null,
        float brightness = 1.0f)
    {
        FilenamePrefix = filenamePrefix;
        FilenameSuffix = filenameSuffix;
        FilenameParserType = filenameParserType;
        Invert = invert;
        DisplayName = displayName;
        LatitudeRange = latitudeRange;
        LongitudeRange = longitudeRange;
        Height = height;
        Crop = crop;
        Brightness = brightness;

        // Convert satellite longitude to lat/long scale of -180 to 180 degrees
        Longitude = longitude.NormaliseLongitude();
        FilenameParser = FilenameParserProvider.GetParser(FilenameParserType, FilenamePrefix, FilenameSuffix);
    }

    private string? FilenamePrefix { get; }
    private string? FilenameSuffix { get; }
    public FilenameParserType FilenameParserType { get; }

    /// <summary>
    ///     Whether pixel intensities in IR images should be inverted to match GOES-R.
    /// </summary>
    public bool Invert { get; }

    public string DisplayName { get; }
    public AngleRange LatitudeRange { get; }
    public AngleRange LongitudeRange { get; }
    public double Longitude { get; }
    public double Height { get; }

    /// <summary>
    ///     Image crop ratio. This is expected to be a four element array of {top, right, bottom, left}.
    /// </summary>
    public double[]? Crop { get; }

    public float Brightness { get; }

    public IFilenameParser FilenameParser { get; }
}