using System.Collections.Immutable;
using System.Collections.ObjectModel;
using FluentValidation;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Sanchez.Processing.Extensions;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Gradients;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
using SixLabors.ImageSharp.PixelFormats;

namespace Sanchez.Processing.Services;

public interface IGradientService
{
    Task InitialiseAsync();
    ReadOnlyCollection<CieLch> GetGradient();
}

public class GradientService : IGradientService
{
    private readonly RenderOptions _options;
    private readonly ColorSpaceConverter _colourSpaceConverter;
    private bool _initialised;
    private ReadOnlyCollection<CieLch>? _interpolatedGradient;

    public ReadOnlyCollection<CieLch> GetGradient()
    {
        if (!_initialised) throw new InvalidOperationException($"Gradient not initialised; call {nameof(InitialiseAsync)} before use");
        return _interpolatedGradient!;
    }

    public GradientService(RenderOptions options)
    {
        _options = options;
        _colourSpaceConverter = new ColorSpaceConverter();
    }

    public async Task InitialiseAsync()
    {
        var json = await File.ReadAllTextAsync(_options.Overlay.GradientPath);
        var gradient = JsonConvert.DeserializeObject<List<RgbColourStop>>(json)!
            .Select(c =>
            {
                if (c.Colour == null) throw new ValidationException("Colour must be specified");
                var colour = c.Colour.FromHexString();
                    
                if (colour == null) throw new ValidationException($"Unable to parse {c.Colour} as a hex triplet");
                if (c.Position == null) throw new ValidationException("Position must be specified for all colour stops");
                if (c.Position is < 0 or > 1) throw new ValidationException($"{c.Position} is an invalid position; valid values are from 0.0 to 1.0");
                    
                var rgb = colour.Value.ToPixel<Rgba32>();
                return new ColourStop(_colourSpaceConverter.ToCieLch(rgb), c.Position.Value);
            })
            .OrderBy(c => c.Position)
            .ToImmutableList();

        _interpolatedGradient = InterpolateGradient(gradient);
        _initialised = true;
    }

    /// <summary>
    ///     Interpolates the gradient to contain 256 values. Interpolation is performed
    ///     using CIE Lch as this performs well for providing aesthetically pleasing and
    ///     predictable gradients between few data points.
    /// </summary>
    private static ReadOnlyCollection<CieLch> InterpolateGradient(IReadOnlyList<ColourStop> gradient)
    {
        var interpolatedGradient = new List<CieLch>();

        var bucketSize = 256f / (gradient.Count - 1);
        for (var bucket = 0; bucket < gradient.Count - 1; bucket++)
        {
            // Create integer bucket sizes which fill full intensity range
            var intBucketSize = (int) (Math.Round((bucket + 1) * bucketSize) - Math.Round((bucket + 0) * bucketSize));

            for (var i = 0; i < intBucketSize; i++)
            {
                var first = gradient[bucket].Colour;
                var second = gradient[bucket + 1].Colour;

                var position = i / (float) (intBucketSize - 1);

                var l = (float) Interpolation.Lerp(first.L, second.L, position);
                var c = (float) Interpolation.Lerp(first.C, second.C, position);
                var h = (float) Interpolation.Lerp(first.H, second.H, position);

                interpolatedGradient.Add(new CieLch(l, c, h));
            }
        }
            
        return interpolatedGradient.AsReadOnly();
    }


    /// <summary>
    ///     Debugging method to save gradient image to disk.
    /// </summary>
    [UsedImplicitly]
    private static Task SaveGradientAsync(string path, List<CieLch> gradient)
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