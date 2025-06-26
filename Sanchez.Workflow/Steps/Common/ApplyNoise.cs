using JetBrains.Annotations;
using Sanchez.Processing.ImageProcessing.Noise;
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
public class ApplyNoise(RenderOptions options) : StepBody
{
    public Image<Rgba32>? Image { get; set; }

    public override ExecutionResult Run(IStepExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(Image);
        
        if (options.Noise)
        {
            Image.ApplyGaussianNoise();
        }

        return ExecutionResult.Next();
    }
}

internal static class ApplyNoiseExtensions
{
    /// <summary>
    ///     Renders gaussian noise to an image.
    /// </summary>
    internal static IStepBuilder<TData, ApplyNoise> ApplyNoise<TStep, TData>(this IStepBuilder<TData, TStep> builder)
        where TStep : IStepBody
        where TData : WorkflowData
        => builder
            .Then<TStep, ApplyNoise, TData>("Apply noise")
            .Input(step => step.Image, data => data.TargetImage);
}