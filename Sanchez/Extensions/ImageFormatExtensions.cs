using System.IO;
using Sanchez.Models;

namespace Sanchez.Extensions
{
    internal static class ImageFormatExtensions
    {
        /// <summary>
        ///     Returns the image format based on the extension of a file.
        /// </summary>
        /// <remarks>
        ///     Currently, only JPEG and PNG files are supported.
        /// </remarks>
        /// <returns>image format, or <c>null</c> if none supported.</returns>
        internal static ImageFormat? GetImageFormat(this string? filename)
        {
            var extension = Path.GetExtension(filename)?.ToLower();
            switch (extension)
            {
                case ".jpeg":
                case ".jpg":
                    return ImageFormat.Jpeg;
                case ".png":
                    return ImageFormat.Png;
                default:
                    return null;
            }
        }
    }
}