﻿using Extend;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
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

namespace Sanchez.Workflow.Steps.Equirectangular;

public sealed class RenderUnderlay(
    RenderOptions options,
    IUnderlayService underlayService,
    ILogger<RenderUnderlay> logger) : StepBodyAsync, IActivityStepBody
{
    public Activity? Activity { get; set; }
    internal Image<Rgba32>? TargetImage { get; [UsedImplicitly] set; }
    public Rectangle? CropBounds { get; [UsedImplicitly] set; }

    public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(Activity);
        ArgumentNullException.ThrowIfNull(TargetImage);

        Activity.GetCropRange(out var latitudeRange, out var longitudeRange);
            
        // Load underlay
        if (options.NoUnderlay)
        {
            // Draw stitched image over black background for non-PNG files because otherwise alpha can look odd
            TargetImage = !Path.GetExtension(options.OutputPath).CompareOrdinalIgnoreCase(".png") ? TargetImage.AddBackgroundColour(Color.Black) : TargetImage;
            return ExecutionResult.Next();
        }

        logger.LogInformation("Tinting and normalising IR imagery");

        TargetImage.Mutate(imageContext =>
        {
            using var clone = TargetImage!.Clone();
            clone.AdjustLevels(options.AdaptiveLevelAdjustment);
            TargetImage.Tint(options.Tint);

            imageContext.DrawImage(clone, PixelColorBlendingMode.HardLight, 0.5f);
        });
            
        var underlayOptions = new UnderlayProjectionData(
            ProjectionType.Equirectangular,
            options.InterpolationType,
            options.UnderlayPath,
            options.ImageSize,
            TargetImage!.Size,
            latitudeRange,
            longitudeRange.Start,
            options.EquirectangularRender?.NoCrop == true || options.EquirectangularRender?.ExplicitCrop == true);

        logger.LogInformation("Retrieving underlay");
        var underlay = await underlayService.GetUnderlayAsync(underlayOptions, null, context.CancellationToken);

        // Render target image onto underlay
        using (TargetImage)
        {
            logger.LogInformation("Blending with underlay");
            underlay.Mutate(ctx => ctx.DrawImage(TargetImage, PixelColorBlendingMode.Screen, 1.0f));
        }
            
        TargetImage = underlay;

        return ExecutionResult.Next();
    }
}
    
internal static class RenderUnderlayExtensions
{
    internal static IStepBuilder<TData, RenderUnderlay> RenderUnderlay<TStep, TData>(this IStepBuilder<TData, TStep> builder)
        where TStep : IStepBody
        where TData : StitchWorkflowData
    {
        return builder
            .Then<TStep, RenderUnderlay, TData>("Render underlay")
            .WithActivity()
            .Input(step => step.TargetImage, data => data.TargetImage)
            .Input(step => step.CropBounds, data => data.CropBounds)
            .Output(data => data.TargetImage, step => step.TargetImage);
    }
}