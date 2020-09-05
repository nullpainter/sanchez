using System.Reflection;
using System.Threading.Tasks;
using ExifLibrary;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Funhouse.Extensions.Images
{
    public static class ImageExtensions
    {
        /// <summary>
        ///     Saves an image, adding EXIF metadata.
        /// </summary>
        public static async Task SaveWithExifAsync(this Image<Rgba32> image, string path)
        {
            await image.SaveAsync(path);

            var version = Assembly.GetExecutingAssembly().GetName().Version;
                        
            var file = await ImageFile.FromFileAsync(path);
            file.Properties.Set(ExifTag.Software, $"Sanchez {version}");
            await file.SaveAsync(path); 
        }
    }
}