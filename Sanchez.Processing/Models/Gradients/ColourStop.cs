using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using SixLabors.ImageSharp.ColorSpaces;

namespace Sanchez.Processing.Models.Gradients;

/// <summary>
///     Single source RGB colour and position on target CLUT gradient.
/// </summary>
public record struct RgbColourStop
{
    [Required]
    [JsonProperty(nameof(Colour))]
    public string? Colour { get; init; }

    [Required]
    [JsonProperty(nameof(Position))]
    public float? Position { get; init; }
}

/// <summary>
///     Single CIE LCH colour and position on target CLUT gradient, created from <see cref="RgbColourStop" /> instance.
/// </summary>
public record struct ColourStop
{
    public ColourStop(CieLch colour, float position)
    {
        Colour = colour;
        Position = position;
    }

    internal CieLch Colour { get; }
    internal float Position { get; }
}