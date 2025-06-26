﻿using Microsoft.Extensions.Logging;
using Sanchez.Processing.Extensions.Images;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Projections;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using Sanchez.Workflow.Models.Steps;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Equirectangular.Stitch;

internal sealed class StitchImages(ILogger<StitchImages> logger, RenderOptions options) : StepBody, IRegistrationStepBody, IActivityStepBody
{
    public Image<Rgba32>? TargetImage { get; set; }
    public Activity? Activity { get; set; }
    public Registration? Registration { get; set; }

    public override ExecutionResult Run(IStepExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(Activity);
        
        TargetImage = new Image<Rgba32>(options.ImageSize * 2, Activity.Registrations[0].Height);
        logger.LogInformation("Output image size: {Width} x {Height} px", TargetImage.Width, TargetImage.Height);

        // Composite all images
        foreach (var registration in Activity.Registrations)
        {
            using (registration)
            {
                var projectionImage = registration.Image;
                ArgumentNullException.ThrowIfNull(projectionImage);

                var location = new Point(registration.OffsetX, 0);
                TargetImage.DrawImageCylindrical(projectionImage, location, PixelColorBlendingMode.Normal, 1.0f);
            }
        }

        return ExecutionResult.Next();
    }
}

internal static class StitchImagesExtensions
{
    internal static IStepBuilder<TData, StitchImages> StitchImages<TStep, TData>(this IStepBuilder<TData, TStep> builder)
        where TStep : IStepBody
        where TData : StitchWorkflowData
    {
        return builder
            .Then<TStep, StitchImages, TData>("Stitch images")
            .WithRegistration()
            .WithActivity()
            .Output(data => data.TargetImage, step => step.TargetImage);
    }
}