using System.ComponentModel.DataAnnotations;
using System.Reflection;
using ExifLibrary;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;

namespace Sanchez.Processing.Extensions.Images;

public static class ImageExtensions
{
    /// <summary>
    ///     Saves an image, adding EXIF metadata.
    /// </summary>
    public static async Task SaveWithExifAsync(this Image<Rgba32> image, string path, CancellationToken ct = default)
    {
        // Create target directory if required
        var targetDirectory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        // Save image, increasing JPEG compression quality
        var encoder = image.DetectEncoder(path);
        if (encoder is JpegEncoder jpegEncoder) jpegEncoder.Quality = 95;
        
        try
        {
            await image.SaveAsync(path, encoder, ct);
        }
        catch (NotSupportedException)
        {
            throw new ValidationException($"Unsupported output file extension: {Path.GetExtension(path)}");
        }

        // Add EXIF metadata to image
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        var file = await ImageFile.FromFileAsync(path);
        file.Properties.Set(ExifTag.Software, $"Sanchez {version}");
        await file.SaveAsync(path);
    }
}