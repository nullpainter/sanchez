using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sanchez.Builders;
using Sanchez.Extensions;
using Sanchez.Factories;
using Sanchez.Models;
using Serilog;
using ShellProgressBar;
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
            List<string> sourceFiles;
            try
            {
                sourceFiles = _fileService.GetSourceFiles(options);
                if (!sourceFiles.Any())
                {
                    Console.Error.WriteLine("No source files found");
                    return;
                }
            }
            catch (DirectoryNotFoundException)
            {
                Console.Error.WriteLine($"Unable to find source directory");
                return;
            }

            // Perform compositing
            var createdImages = 0;
            var ignoredFiles = 0;
            var existingFiles = 0;

            var progressBarOptions = new ProgressBarOptions
            {
                ProgressCharacter = '─',
                ProgressBarOnBottom = true
            };

            using (var progressBar = NewProgressBar(options, sourceFiles, progressBarOptions))
            {
                Parallel.ForEach(sourceFiles, new ParallelOptions
                    {
                        MaxDegreeOfParallelism = Environment.ProcessorCount
                    },
                    file =>
                    {
                        try
                        {
                            // ReSharper disable once AccessToDisposedClosure
                            progressBar.Message = $"Compositing {file}";

                            // Perform compositing
                            var outputPath = CompositeImage(options, renderOptions, file);
                            if (outputPath != null)
                            {
                                Interlocked.Increment(ref createdImages);
                                Log.Information("Source file {image} completed processing. Output written to {outputPath}", file, outputPath);
                            }
                            else
                            {
                                Interlocked.Increment(ref existingFiles);
                            }
                        }
                        catch (UnknownImageFormatException)
                        {
                            Interlocked.Increment(ref ignoredFiles);
                            Log.Error("Unable to compose image {image} due to corrupt or unhandled source", file);
                        }
                        catch (Exception e)
                        {
                            Interlocked.Increment(ref ignoredFiles);
                            Log.Error(e, "Unhandled error composing image {image}", file);
                        }
                        finally
                        {
                            // ReSharper disable once AccessToDisposedClosure
                            progressBar.Tick();
                        }
                    }
                );

                progressBar.Message = "Done";
            }

            Console.WriteLine();
            Console.WriteLine(createdImages > 0 ? $"Composited {createdImages} images" : "0 images composited");
            if (ignoredFiles > 0) Console.WriteLine($"{ignoredFiles} files ignored");
            if (existingFiles > 0) Console.WriteLine($"{existingFiles} files already composited");
        }

        private static IProgressBar NewProgressBar(CommandLineOptions options, ICollection sourceFiles, ProgressBarOptions progressBarOptions)
        {
            if (options.Quiet || sourceFiles.Count <= 1) return new NullProgressBar();
            return new ProgressBar(sourceFiles.Count, "Compositing files", progressBarOptions);
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
                Log.Information("Output file {outputFilename} exists; not overwriting", outputFilename);
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