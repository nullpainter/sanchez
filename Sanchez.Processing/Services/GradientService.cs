using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using FluentValidation;
using Newtonsoft.Json;
using Sanchez.Processing.Extensions;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Gradients;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.ColorSpaces.Conversion;

namespace Sanchez.Processing.Services;

public interface IGradientService
{
    ReadOnlyCollection<CieLch> GetGradient(string? path = null);
    Task SaveGradientAsync(string path, ReadOnlyCollection<CieLch> gradient);
}

public class GradientService : IGradientService
{
    private readonly RenderOptions _options;
    private readonly ColorSpaceConverter _colourSpaceConverter;
    private readonly ConcurrentDictionary<string, ReadOnlyCollection<CieLch>> _cache;

    private const int GradientSize = 256;

    public GradientService(RenderOptions options)
    {
        _options = options;

        _cache = new ConcurrentDictionary<string, ReadOnlyCollection<CieLch>>();
        _colourSpaceConverter = new ColorSpaceConverter();
    }

    public ReadOnlyCollection<CieLch> GetGradient(string? path = null)
        => _cache.GetOrAdd(path ?? _options.Overlay.GradientPath, CreateGradient);

    private ReadOnlyCollection<CieLch> CreateGradient(string path)
    {
        var json = File.ReadAllText(path);
        var gradient = JsonConvert.DeserializeObject<List<RgbColourStop>>(json)!
            .Select(c =>
            {
                if (c.Colour == null) throw new ValidationException("Colour must be specified");
                var colour = c.Colour.FromHexString();

                if (colour == null) throw new ValidationException($"Unable to parse {c.Colour} as a hex triplet");
                switch (c.Position)
                {
                    case null:
                        throw new ValidationException("Position must be specified for all colour stops");
                    case < 0 or > 1:
                        throw new ValidationException($"{c.Position} is an invalid position; valid values are from 0.0 to 1.0");
                    default:
                    {
                        var rgb = colour.Value.ToPixel<Rgba32>();
                        return new ColourStop(_colourSpaceConverter.ToCieLch(rgb), c.Position.Value);
                    }
                }
            })
            .OrderBy(c => c.Position)
            .ToImmutableList();

        return InterpolateGradient(gradient);
    }

    /// <summary>
    ///     Interpolates the gradient to contain 256 values. Interpolation is performed
    ///     using CIE Lch as this performs well for providing aesthetically pleasing and
    ///     predictable gradients between few data points.
    /// </summary>
    private static ReadOnlyCollection<CieLch> InterpolateGradient(IReadOnlyList<ColourStop> gradient)
    {
        var interpolatedGradient = new List<CieLch>();
        var totalSize = 0;

        for (var bucket = 0; bucket < gradient.Count - 1; bucket++)
        {
            var bucketSize = gradient[bucket + 1].Position - gradient[bucket].Position;
            var intBucketSize = (int)Math.Round(bucketSize * GradientSize);

            totalSize += intBucketSize;

            // Ensure gradient has 256 entries. Too few entries can be caused by gradient positions which are too close together
            if (bucket == gradient.Count - 2 && totalSize < GradientSize)
            {
                intBucketSize += GradientSize - totalSize;
            }

            for (var i = 0; i < intBucketSize; i++)
            {
                var first = gradient[bucket].Colour;
                var second = gradient[bucket + 1].Colour;

                var position = i / (float)(intBucketSize - 1);

                var l = (float)Interpolation.Lerp(first.L, second.L, position);
                var c = (float)Interpolation.Lerp(first.C, second.C, position);
                var h = (float)Interpolation.Lerp(first.H, second.H, position);

                interpolatedGradient.Add(new CieLch(l, c, h));
            }
        }

        return interpolatedGradient.AsReadOnly();
    }


    /// <summary>
    ///     Debugging method to save gradient image to disk.
    /// </summary>
    public Task SaveGradientAsync(string path, ReadOnlyCollection<CieLch> gradient)
    {
        var converter = new ColorSpaceConverter();
        var image = new Image<Rgba32>(gradient.Count, 50);
        const int height = 50;

        for (var x = 0; x < gradient.Count; x++)
        {
            for (var y = 0; y < height; y++)
            {
                image[x, y] = converter.ToRgb(gradient[x]);
            }
        }

        return image.SaveAsync(path);
    }
}