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

namespace Sanchez.Workflow.Steps.Geostationary;

internal sealed class RenderUnderlay(
    ILogger<RenderUnderlay> logger,
    IUnderlayService underlayService,
    RenderOptions options) : StepBodyAsync, IRegistrationStepBody
{
    public Registration? Registration { get; set; }
    internal Image<Rgba32>? TargetImage { get; private set; }

    public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(options.GeostationaryRender);
        ArgumentNullException.ThrowIfNull(Registration?.Image);
            
        if (options.NoUnderlay)
        {
            TargetImage = Registration.Image.Clone();
            return ExecutionResult.Next();
        }
            
        // Get or generate projected underlay
        var underlayOptions = new UnderlayProjectionData(
            ProjectionType.Geostationary,
            options.InterpolationType,
            options.UnderlayPath,
            options.ImageSize);

        logger.LogInformation("Retrieving underlay");
        var underlay = await underlayService.GetUnderlayAsync(underlayOptions, Registration.Definition);

        logger.LogInformation("Tinting and normalising IR imagery");
        if (options.AutoAdjustLevels) Registration.Image.AdjustLevels(options.AdaptiveLevelAdjustment);

        TargetImage = Registration.Image.Clone();
        TargetImage.Tint(options.Tint);

        logger.LogInformation("Blending with underlay");
        TargetImage.Mutate(c => c
            .Resize(options.ImageSize, options.ImageSize)
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