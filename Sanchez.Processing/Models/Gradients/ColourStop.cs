using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using SixLabors.ImageSharp.ColorSpaces;

namespace Sanchez.Processing.Models.Gradients
{
    /// <summary>
    ///     Single source RGB colour and position on target CLUT gradient.
    /// </summary>
    public class RgbColourStop
    {
        [Required]
        [JsonProperty("Colour")]
        public string? Colour { get; set; }

        [Required]
        [JsonProperty("Position")]
        public float? Position { get; set; }
    }

    /// <summary>
    ///     Single CIE LCH colour and position on target CLUT gradient, created from <see cref="RgbColourStop" /> instance.
    /// </summary>
    public class ColourStop
    {
        public ColourStop(CieLch colour, float position)
        {
            Colour = colour;
            Position = position;
        }

        internal CieLch Colour { get; }
        internal float Position { get; }
    }
}