using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sanchez.Builders;
using Sanchez.Extensions;
using Sanchez.Factories;
using Sanchez.Models;
using SixLabors.ImageSharp;

namespace Sanchez.Services
{
    public interface ICompositor
    {
        /// <summary>
        ///     Creates a composite image from a coloured underlay image, satellite IR image and optional mask.
        /// </summary>
        /// <remarks>
        ///     It is assumed that all images are the same dimensions.
        /// </remarks>
        void Create(CommandLineOptions options);
    }

    public class Compositor : ICompositor
    {
        private readonly IFileService _fileService;

        public Compositor(IFileService fileService) => _fileService = fileService;

        /// <summary>
        ///     Creates a composite image from a coloured underlay image, satellite IR image and optional mask.
        /// </summary>
        /// <remarks>
        ///     It is assumed that all images are the same dimensions.
        /// </remarks>
        public void Create(CommandLineOptions options)
        {
            var renderOptions = RenderOptionFactory.ToRenderOptions(options);

            // Verify selected tint
            if (renderOptions.Tint == null)
            {
                Console.Error.WriteLine("Unable to parse tint as a hex tuple. Expected format is 5ebfff");
                return;
            }

            // Get source satellite files
            var sourceFiles = _fileService.GetSourceFiles(options);
            if (!sourceFiles.Any())
            {
                Console.Error.WriteLine("No source files found");
                return;
            }

            // Perform compositing
            var stopwatch = Stopwatch.StartNew();
            int createdImages = 0;
            
            Parallel.ForEach(sourceFiles, new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                },
                file =>
                {
                    var outputPath = CompositeImage(options, renderOptions, file);
                    if (outputPath != null)
                    {
                        Interlocked.Increment(ref createdImages);
                        Console.WriteLine($"Output written to {outputPath}");
                    }
                }
            );

            if (createdImages > 0)
            {
                Console.WriteLine($"Created {createdImages} images in {stopwatch.Elapsed.TotalSeconds:F3} seconds");
            }
            else
            {
                Console.WriteLine("Created no images");
            }
        }

        /// <summary>
        ///     Composes the image, returning the path to the output file if written.
        /// </summary>
        private string? CompositeImage(CommandLineOptions options, RenderOptions renderOptions, string sourcePath)
        {
            _fileService.PrepareOutput(options);

            // Prepare output filename. For a single source, this will be the filename provided by the user. For a bulk
            // source, this will be the name of the source file with a suffix.
            var outputFilename = _fileService.GetOutputFilename(options, sourcePath!);

            // Verify that the output file doesn't already exist and that the target folder isn't a file if using a bulk source
            if (options.IsBatch && File.Exists(options.OutputPath) || File.Exists(outputFilename))
            {
                Console.Error.WriteLine($"Output file {outputFilename} exists; not overwriting");
                return null;
            }

            // Load underlay image
            using var underlay = Image.Load(options.UnderlayPath);

            // Load satellite image, removing grey tint
            using var satelliteImage = Image.Load(sourcePath);
            satelliteImage.TintAndBlend(renderOptions.Tint!.Value);

            // Composite images
            new CompositorBuilder(underlay, renderOptions)
                .AddUnderlay(satelliteImage)
                .PostProcess()
                .AddMask(options.MaskPath)
                .AddOverlay(options.OverlayPath)
                .Save(outputFilename);

            return Path.GetFullPath(outputFilename);
        }
    }
}