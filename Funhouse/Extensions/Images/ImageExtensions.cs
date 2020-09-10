using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using ExifLibrary;
using Funhouse.Models;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Funhouse.Extensions.Images
{
    public static class ImageExtensions
    {
        /// <summary>
        ///     Saves an image, adding EXIF metadata.
        /// </summary>
        public static async Task SaveWithExifAsync(this Image<Rgba32> image, string path, RenderOptions options)
        {
            // Verify that the output file doesn't already exist and that the target folder isn't a file if using a bulk source
            if (!ShouldWrite(path, options))
            {
                // TODO should write console too
                Log.Information("Output file {outputFilename} exists; not overwriting", path);
                return;
            }

            // Save image
            await image.SaveAsync(path);

            var version = Assembly.GetExecutingAssembly().GetName().Version;

            var file = await ImageFile.FromFileAsync(path);

            file.Properties.Set(ExifTag.Software, $"Sanchez {version}");
            await file.SaveAsync(path);


            if (options.Verbose)
            {
                Log.Information("Output written to {path}", Path.GetFullPath(path));
            }
            else
            {
                Console.WriteLine($"Output written to {Path.GetFullPath(path)}");
            }
        }

        /// <summary>
        ///     Whether the output file should be written, based on options and whether the file already exists.
        /// </summary>
        private static bool ShouldWrite(string path, RenderOptions options)
        {
            if (options.Force) return true;
            return !File.Exists(path);
        }
    }
}