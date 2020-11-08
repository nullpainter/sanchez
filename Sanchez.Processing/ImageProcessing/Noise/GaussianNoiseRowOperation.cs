using System;
using Sanchez.Processing.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace Sanchez.Processing.ImageProcessing.Noise
{
    public struct GaussianNoiseRowOperation : IRowOperation
    {
        private readonly Image<Rgba32> _image;

        private const int Mean = 0;
        private const int StdDev = 1;

        public GaussianNoiseRowOperation(Image<Rgba32> image) => _image = image;

        public void Invoke(int y)
        {
            var random = new Random();

            var span = _image.GetPixelRowSpan(y);

            for (var x = 0; x < span.Length; x++)
            {
                var u1 = 1.0 - random.NextDouble();
                var u2 = 1.0 - random.NextDouble();

                var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(Constants.Pi2 * u2);

                var randNormal = (float) (Mean + StdDev * randStdNormal);

                span[x] = new Rgba32(randNormal, randNormal, randNormal, 1); //span[x].A);
            }
        }
    }
}