using System;
using System.Collections.Generic;
using Sanchez.Processing.Extensions;
using Sanchez.Processing.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
using SixLabors.ImageSharp.PixelFormats;

namespace Sanchez.Processing.Services
{
    public interface IClutService
    {
        List<Rgba32> GetClut();
    }


    public class ClutService : IClutService
    {
        private readonly IGradientService _gradientService;
        private readonly ColorSpaceConverter _converter;
        private readonly RenderOptions _options;

        public ClutService(IGradientService gradientService, RenderOptions options)
        {
            _gradientService = gradientService;
            _options = options;
            _converter = new ColorSpaceConverter();
        }

        public List<Rgba32> GetClut()
        {
            var gradient = _gradientService.GetGradient();

            var lookupTable = new List<Rgba32>();
            for (var i = 0; i < 256; i++)
            {
                var colour = GetColour(gradient, (byte) i);
                lookupTable.Add(colour);
            }

            return lookupTable;
        }

        private Rgba32 GetColour(IReadOnlyList<CieLch> gradient, byte index)
        {
            var overlayOptions = _options.Overlay;
            var maxIntensity = overlayOptions.MaxIntensity;
            var minIntensity = overlayOptions.MinIntensity;

            var temperature = GetTemperature(index);
            var range = maxIntensity - minIntensity;

            if (temperature < minIntensity || temperature > maxIntensity) return Color.Transparent;

            var interpolateAmount = (float) (temperature - minIntensity) / range;
            var cieLch = gradient[(int) Math.Round(interpolateAmount * (gradient.Count - 1))];

            return _converter.ToRgb(cieLch);
        }

        private static double GetTemperature(byte intensity) => Interpolation.Lerp(0, 1, intensity / 255f);
    }
}