using System;
using System.IO;
using System.Runtime.CompilerServices;
using CommandLine;
using Sanchez.Extensions;
using Sanchez.Models;
using Sanchez.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using Color = System.Drawing.Color;

[assembly: InternalsVisibleTo("Sanchez.Test")]
namespace Sanchez
{
    internal static class Sanchez
    {
        /// <summary>
        ///     Main entry point to application, parsing command-line arguments and creating composite image.
        /// </summary>
        internal static void Main(params string[] args) => Parser.Default.ParseArguments<CommandLineOptions>(args).WithParsed(Process);

        /// <summary>
        ///     Creates a composite image from a coloured underlay image, satellite IR image and optional mask. 
        /// </summary>
        /// <remarks>
        ///    It is assumed that all images are the same dimensions.
        /// </remarks>
        private static void Process(CommandLineOptions options)
        {
            // Verify arguments
            if (!CommandLineValidator.VerifyPaths(options, out var invalidPaths))
            {
                invalidPaths.ForEach(path => Console.Error.WriteLine($"Path {path} isn't valid."));
                Environment.Exit(-1);
            }

            // Parse and verify selected tint
            var tint = options.Tint.FromHexTriplet();
            if (tint == null)
            {
                Console.Error.WriteLine("Unable to parse tint as a hex tuple. Expected format is 5ebfff");
                Environment.Exit(-1);
                return;
            }

            // Perform compositing
            var outputPath = CompositeImage(options, tint.Value);
            Console.WriteLine($"Output written to {outputPath}");
        }

        private static string CompositeImage(CommandLineOptions options, Color tint)
        {
            using var underlay = Image.Load(options.UnderlayPath);
            using var outputStream = new FileStream(options.OutputFile!, FileMode.Create);

            underlay.Mutate(context =>
            {
                // Load satellite image, removing grey tint
                using var satelliteImage = Image.Load(options.SourceImagePath);
                satelliteImage.TintSatelliteImage(tint);

                // Combine images
                context
                    .BlendUnderlayImage(satelliteImage)
                    .PostProcess(options)
                    .AddMask(options);
            });

            // Save output file as a JPEG
            // TODO honour and validate file extension to allow saving as PNG
            var encoder = new JpegEncoder { Quality = 85 };
            underlay.SaveAsJpeg(outputStream, encoder);
            
            return outputStream.Name;
        }
    }
}