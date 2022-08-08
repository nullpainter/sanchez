using Sanchez.Processing.ImageProcessing.Haze;
using Sanchez.Processing.Models;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Geostationary;

public class ApplyHaze : StepBody
{
    private readonly RenderOptions _options;

    public ApplyHaze(RenderOptions options) => _options = options;

    public Image<Rgba32>? TargetImage { get; set; }

    public override ExecutionResult Run(IStepExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(TargetImage);
        ArgumentNullException.ThrowIfNull(_options.GeostationaryRender);
            
        // Only apply haze with underlay
        if (_options.NoUnderlay) return ExecutionResult.Next();

        var hazeAmount = _options.GeostationaryRender.HazeAmount;
        var hazeOpacity = _options.GeostationaryRender.HazeOpacity;
            
        if (hazeAmount > 0 && hazeOpacity > 0) TargetImage.ApplyHaze(_options.Tint, hazeAmount, hazeOpacity);

        return ExecutionResult.Next();
    }
}

internal static class ApplyHazeExtensions
{
    internal static IStepBuilder<TData, ApplyHaze> ApplyHaze<TStep, TData>(this IStepBuilder<TData, TStep> builder)
        where TStep : IStepBody
        where TData : WorkflowData
        => builder
            .Then<TStep, ApplyHaze, TData>("Apply haze")
            .Input(step => step.TargetImage, data => data.TargetImage);
}