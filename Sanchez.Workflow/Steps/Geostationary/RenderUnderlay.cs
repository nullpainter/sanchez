﻿using Microsoft.Extensions.Logging;
using Sanchez.Processing.Extensions.Images;
using Sanchez.Processing.ImageProcessing.Tint;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Projections;
using Sanchez.Processing.Services.Underlay;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using Sanchez.Workflow.Models.Steps;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Geostationary;

internal sealed class RenderUnderlay : StepBodyAsync, IRegistrationStepBody
{
    private readonly ILogger<RenderUnderlay> _logger;
    private readonly IUnderlayService _underlayService;
    private readonly RenderOptions _options;
        
    public Registration? Registration { get; set; }
    internal Image<Rgba32>? TargetImage { get; private set; }

    public RenderUnderlay(
        ILogger<RenderUnderlay> logger,
        IUnderlayService underlayService, 
        RenderOptions options)
    {
        _logger = logger;
        _underlayService = underlayService;
        _options = options;
    }

    public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(_options.GeostationaryRender);
        ArgumentNullException.ThrowIfNull(Registration?.Image);
            
        if (_options.NoUnderlay)
        {
            TargetImage = Registration.Image.Clone();
            return ExecutionResult.Next();
        }
            
        // Get or generate projected underlay
        var underlayOptions = new UnderlayProjectionData(
            ProjectionType.Geostationary,
            _options.InterpolationType,
            _options.UnderlayPath,
            _options.ImageSize);

        _logger.LogInformation("Retrieving underlay");
        var underlay = await _underlayService.GetUnderlayAsync(underlayOptions, Registration.Definition);

        _logger.LogInformation("Tinting and normalising IR imagery");
        if (_options.AutoAdjustLevels) Registration.Image.AdjustLevels(_options.AdaptiveLevelAdjustment);

        TargetImage = Registration.Image.Clone();
        TargetImage.Tint(_options.Tint);

        _logger.LogInformation("Blending with underlay");
        TargetImage.Mutate(c => c
            .Resize(_options.ImageSize, _options.ImageSize)
            .DrawImage(underlay, PixelColorBlendingMode.Screen, 1.0f));

        return ExecutionResult.Next();
    }
}

internal static class RegisterUnderlayExtensions
{
    internal static IStepBuilder<GeostationaryWorkflowData, RenderUnderlay> RenderUnderlay<TStep>(this IStepBuilder<GeostationaryWorkflowData, TStep> builder)
        where TStep : IStepBody
        => builder
            .Then<TStep, RenderUnderlay, GeostationaryWorkflowData>("Render underlay")
            .WithRegistration()
            .Output(data => data.TargetImage, step => step.TargetImage);
}