using System.Threading.Tasks;
using Funhouse.Models.Projections;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Funhouse.Services
{
    public interface IImageLoader
    {
        Task<ProjectionActivity> LoadAsync(string path);
    }

    public class ImageLoader : IImageLoader
    {
        private readonly ISatelliteRegistry _satelliteRegistry;

        public ImageLoader(ISatelliteRegistry satelliteRegistry) => _satelliteRegistry = satelliteRegistry;

        public async Task<ProjectionActivity> LoadAsync(string path)
        {
            var projectedImage = new ProjectionActivity(path)
            {
                Source = await Image.LoadAsync<Rgba32>(path),
                Definition = _satelliteRegistry.Locate(path)
            };

            return projectedImage;
        }
    }
}