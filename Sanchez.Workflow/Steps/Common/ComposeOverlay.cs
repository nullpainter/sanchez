using JetBrains.Annotations;
using Sanchez.Processing.Models;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Common;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class ComposeOverlay(RenderOptions options) : StepBody
{
    public Image<Rgba32>? OverlayImage { get; set; }
    public Image<Rgba32>? TargetImage { get; set; }

    public override ExecutionResult Run(IStepExecutionContext context)
    {
        if (!options.Overlay.ApplyOverlay) return ExecutionResult.Next();

        ArgumentNullException.ThrowIfNull(OverlayImage);
        ArgumentNullException.ThrowIfNull(TargetImage);

        TargetImage.Mutate(c => c.DrawImage(OverlayImage, PixelColorBlendingMode.Normal, 1.0f));
            
        return ExecutionResult.Next();
    }
}

internal static class ComposeOverlayExtensions
{
    /// <summary>
    ///     Renders an overlay on a registration image.
    /// </summary>
    internal static IStepBuilder<TData, ComposeOverlay> ComposeOverlay<TStep, TData>(this IStepBuilder<TData, TStep> builder)
        where TStep : IStepBody
        where TData : WorkflowData
        => builder
            .Then<TStep, ComposeOverlay, TData>("Compose overlay")
            .Input(step => step.TargetImage, data => data.TargetImage)
            .Input(step => step.OverlayImage, data => data.OverlayImage);
}