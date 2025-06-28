using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Angles;
using Sanchez.Processing.Models.Configuration;

namespace Sanchez.Processing.Services;

public interface ISatelliteRegistry
{
    Task InitialiseAsync();
    (SatelliteDefinition? definition, DateTime? timestamp) Locate(string path);
}

public class SatelliteRegistry(
    IVisibleRangeService visibleRangeService,
    ILogger<SatelliteRegistry> logger,
    RenderOptions options) : ISatelliteRegistry
{
    private List<SatelliteDefinition>? _definitions;
    private bool _initialised;

    public async Task InitialiseAsync()
    {
        logger.LogInformation("Initialising satellite registry from {DefinitionsPath}", options.DefinitionsPath);
        
        var json = await File.ReadAllTextAsync(options.DefinitionsPath);
        var definitions = JsonConvert.DeserializeObject<List<SatelliteConfiguration>>(json);

        _definitions = definitions!.Select(d => new SatelliteDefinition(
            d.DisplayName,
            d.FilenamePrefix,
            d.FilenameSuffix,
            d.FilenameParserType,
            d.Invert,
            Angle.FromDegrees(d.Longitude + d.LongitudeAdjustment.GetValueOrDefault()).Radians,
            new AngleRange(
                Angle.FromDegrees(Constants.Satellite.CropRange.MinLatitude),
                Angle.FromDegrees(Constants.Satellite.CropRange.MaxLatitude)),
            visibleRangeService.GetVisibleRange(Angle.FromDegrees(d.Longitude)),
            d.Height,
            d.Crop,
            d.Brightness)).ToList();

        await InitializeImagePathsAsync();

        _initialised = true;
    }

    /// <summary>
    ///     Initialise base directories for each satellite definition, if provided.
    /// </summary>
    private async Task InitializeImagePathsAsync()
    {
        ArgumentNullException.ThrowIfNull(_definitions);

        if (options.ImagePaths == null) return;
        
        logger.LogInformation("Initialising image paths from {ImagePaths}", options.ImagePaths);
        var json = await File.ReadAllTextAsync(options.ImagePaths);

        var paths = JsonConvert.DeserializeObject<List<ImagePathConfiguration>>(json)!;

        foreach (var path in paths)
        {
            // Validate satellite definition exists
            var definition = _definitions.Find(d => d.DisplayName == path.Satellite);
            if (definition == null)
            {
                logger.LogWarning("Unable to find satellite definition for {Satellite}; ignoring image path", path.Satellite);
                continue;
            }

            definition.RootDirectory = Path.GetFullPath(path.Directory);
        }
    }

    public (SatelliteDefinition? definition, DateTime? timestamp) Locate(string path)
    {
        if (!_initialised) throw new InvalidOperationException($"Registry not initialised; call {nameof(InitialiseAsync)} before use");

        var fullPath = Path.GetFullPath(path);
        var filename = Path.GetFileName(fullPath);

        foreach (var definition in _definitions!)
        {
            // If a root directory is specified, verify that the path is under that directory
            if (definition.RootDirectory != null &&
                !fullPath.StartsWith(definition.RootDirectory, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogTrace(
                    "Skipping {Definition} handler for {Filename} as it is not under the root directory {RootDirectory}",
                    definition.DisplayName, filename, definition.RootDirectory);
                
                continue;
            }

            // Attempt to extract a timestamp from the filename using the definition's parser
            var timestamp = definition.FilenameParser.GetTimestamp(filename);
            if (timestamp == null) continue;

            logger.LogDebug("Matched {Definition} handler for {Filename}", definition.DisplayName, filename);
            return (definition, timestamp);
        }

        logger.LogWarning("Unable to find filename parser for {Filename}", filename);

        return (null, null);
    }
}