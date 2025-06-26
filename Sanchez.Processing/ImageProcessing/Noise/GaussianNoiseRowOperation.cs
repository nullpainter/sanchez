using System.Numerics;
using Sanchez.Processing.Models;
using SixLabors.ImageSharp.PixelFormats;

namespace Sanchez.Processing.ImageProcessing.Noise;

public static class GaussianNoiseRowOperation
{
    private const int Mean = 0;
    private const int StdDev = 1;

    public static void Invoke(Span<Vector4> row)
    {
        var random = new Random();

        for (var x = 0; x < row.Length; x++)
        {
            var u1 = 1.0 - random.NextDouble();
            var u2 = 1.0 - random.NextDouble();

            var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(Constants.Pi2 * u2);

            var randNormal = (float)(Mean + StdDev * randStdNormal);

            row[x] = new Rgba32(randNormal, randNormal, randNormal).ToVector4();
        }
    }
}