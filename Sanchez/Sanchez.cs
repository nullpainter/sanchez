using System;
using System.IO;
using System.Runtime.CompilerServices;
using CommandLine;
using Sanchez.Builders;
using Sanchez.Extensions;
using Sanchez.Factories;
using Sanchez.Models;
using Sanchez.Services;
using SixLabors.ImageSharp;

[assembly: InternalsVisibleTo("Sanchez.Test")]

namespace Sanchez
{
    internal static class Sanchez
    {
        /// <summary>
        ///     Main entry point to application, parsing command-line arguments and creating composite image.
        /// </summary>
        internal static void Main(params string[] args) => Parser.Default.ParseArguments<CommandLineOptions>(args).WithParsed(options =>
        {
            var renderOptions = RenderOptionFactory.ToRenderOptions(options);
            if (CommandLineValidator.Validate(options, renderOptions))
            {
                Process(options, renderOptions);
            }
        });

        /// <summary>
        ///     Creates a composite image from a coloured underlay image, satellite IR image and optional mask.
        /// </summary>
        /// <remarks>
        ///     It is assumed that all images are the same dimensions.
        /// </remarks>
        private static void Process(CommandLineOptions options, RenderOptions renderOptions)
        {
            // Verify selected tint
            if (renderOptions.Tint == null)
            {
                Console.Error.WriteLine("Unable to parse tint as a hex tuple. Expected format is 5ebfff");
                Environment.Exit(-1);
                return;
            }

            // Perform compositing
            var outputPath = CompositeImage(options, renderOptions);
            Console.WriteLine($"Output written to {outputPath}");
        }

        private static string CompositeImage(CommandLineOptions options, RenderOptions renderOptions)
        {
            using var underlay = Image.Load(options.UnderlayPath);

            // Load satellite image, removing grey tint
            using var satelliteImage = Image.Load(options.SourceImagePath);
            satelliteImage.TintAndBlend(renderOptions.Tint!.Value);

            // Composite images
            var outputFile = new CompositorBuilder(underlay, renderOptions)
                .AddUnderlay(satelliteImage)
                .PostProcess()
                .AddMask(options.MaskPath)
                .AddOverlay(options.OverlayPath)
                .Save(options.OutputFile!);

            return outputFile;
        }
    }
}