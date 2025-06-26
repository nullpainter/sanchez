﻿using System.Collections.ObjectModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using CommandLine;
using Extend;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Sanchez.Builders;
using Sanchez.Models.CommandLine;
using Sanchez.Processing.Models;
using Sanchez.Services;
using Sanchez.Validators;
using Sanchez.Workflow.Services;
using Serilog;

[assembly: InternalsVisibleTo("Sanchez.Test")]
namespace Sanchez;

internal static class Bootstrapper
{
    internal static async Task<int> Main(params string[] args)
    {
        RenderOptions renderOptions = null!;

        try
        {
            var cancellationToken = new CancellationTokenSource();

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new Collection<JsonConverter> { new StringEnumConverter() }
            };

            var parser = new Parser(with =>
                {
                    with.CaseInsensitiveEnumValues = true;
                    with.HelpWriter = Console.Error;
                })
                .ParseArguments<GeostationaryOptions, EquirectangularOptions>(args)
                .WithParsed<EquirectangularOptions>(options => renderOptions = ParseReprojectOptions(options))
                .WithParsed<GeostationaryOptions>(options => renderOptions = ParseGeostationaryOptions(options));

            // Exit if required options not present
            if (parser.Tag == ParserResultType.NotParsed) throw new ValidationException("Unable to parse command line");
            ArgumentNullException.ThrowIfNull(renderOptions);

            // Disable stdout if required
            if (renderOptions.Quiet) Console.SetOut(TextWriter.Null);

            // Build DI container
            var serviceProvider = ServiceProviderFactory.ConfigureServices(renderOptions);

            Log.Information("Sanchez starting with arguments: {Arguments}", string.Join(' ', args));
            LogOptions(renderOptions);

            // Initialise workflow host
            var workflowService = serviceProvider.GetRequiredService<IWorkflowService>();
            workflowService.Initialise(cancellationToken);

            // Start the workflow 
            await workflowService.StartAsync(cancellationToken);
        }
        catch (ValidationException e)
        {
            Log.Warning(e, "No image processing possible");

            if (e.Errors != null) e.Errors.ForEach(error => Console.Error.WriteLine(error.ErrorMessage));
            else if (!string.IsNullOrEmpty(e.Message)) await Console.Error.WriteLineAsync(e.Message);

            return -1;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            if (!renderOptions.Verbose) Console.WriteLine("Unhandled failure; check logs for details, or run again with verbose logging (-v / --verbose)");
            Log.Error(e, "Unhandled failure");

            return -1;
        }
        finally
        {
            Console.ResetColor();

            // Explicitly enable the cursor, as the progress bar can remove it in some situations
            try
            {
                Console.CursorVisible = true;
            }
            catch (IOException)
            {
                // Ignored
            }
        }

        return 0;
    }

    private static RenderOptions ParseGeostationaryOptions(GeostationaryOptions options)
    {
        var validation = new GeostationaryOptionsValidator().Validate(options);
        if (validation.IsValid) return OptionsParser.Populate(options);

        throw new ValidationException(validation.Errors);
    }

    private static RenderOptions ParseReprojectOptions(EquirectangularOptions options)
    {
        var validation = new EquirectangularOptionsValidator().Validate(options);
        if (validation.IsValid) return OptionsParser.Populate(options);

        throw new ValidationException(validation.Errors);
    }

    private static void LogOptions(RenderOptions options)
    {
        if (options.EquirectangularRender?.AutoCrop == true) Log.Information("Autocrop enabled");
        Log.Information("Using {Type} interpolation", options.InterpolationType);
        Log.Information("Normalising images to {Km} km spatial resolution", options.SpatialResolution);
        Log.Information("Using underlay path {Path}", options.UnderlayPath);
        Log.Information("Using satellite definitions {Path}", options.DefinitionsPath);

        if (options.GeostationaryRender != null)
        {
            Log.Information("Apply {Atmosphere} atmosphere", options.GeostationaryRender.AtmosphereAmount);
        }
    }
}