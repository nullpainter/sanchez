using System.Linq.Expressions;
using JetBrains.Annotations;
using Sanchez.Processing.Extensions.Images;
using Sanchez.Processing.ImageProcessing.Lookup;
using Sanchez.Processing.Models;
using Sanchez.Processing.Services;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Common;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class RenderOverlay(ILookupService lookupService, RenderOptions options) : StepBody
{
    public Image<Rgba32>? SourceImage { get; set; }
    public Image<Rgba32>? OverlayImage { get; set; }

    public override ExecutionResult Run(IStepExecutionContext context)
    {
        if (!options.Overlay.ApplyOverlay) return ExecutionResult.Next();
        
        ArgumentNullException.ThrowIfNull(SourceImage);
        var lookup = lookupService.GetLookup();

        var equalisedSource = SourceImage.Clone();
        equalisedSource.AdjustLevels(options.AdaptiveLevelAdjustment);

        OverlayImage = new Image<Rgba32>(equalisedSource.Width, equalisedSource.Height);

        var operation = new ApplyLookupRowOperation(equalisedSource, lookup);
        equalisedSource.ProcessPixelRows(OverlayImage, (sourceAccessor, targetAccessor) => operation.Invoke(sourceAccessor, targetAccessor));

        return ExecutionResult.Next();
    }
}

internal static class RenderOverlayExtensions
{
    /// <summary>
    ///     Renders an overlay on a registration image.
    /// </summary>
    internal static IStepBuilder<TData, RenderOverlay> RenderOverlay<TStep, TData>(this IStepBuilder<TData, TStep> builder)
        where TStep : IStepBody
        where TData : WorkflowData
        => builder
            .Then<TStep, RenderOverlay, TData>("Render overlay")
            .Input(step => step.SourceImage, data => data.Registration!.Image)
            .Output(data => data.OverlayImage, step => step.OverlayImage);

    internal static IStepBuilder<TData, RenderOverlay> RenderOverlay<TStep, TData>(
        this IStepBuilder<TData, TStep> builder, Expression<Func<TData, Image<Rgba32>?>>? image)
        where TStep : IStepBody
        where TData : WorkflowData
        => builder
            .Then<TStep, RenderOverlay, TData>("Render overlay")
            .Input(step => step.SourceImage, image)
            .Output(data => data.OverlayImage, step => step.OverlayImage);
}