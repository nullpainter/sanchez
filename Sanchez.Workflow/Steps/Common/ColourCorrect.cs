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

public class ColourCorrect(RenderOptions options) : StepBody
{
    public Image<Rgba32>? TargetImage { get; [UsedImplicitly] set; }

    public override ExecutionResult Run(IStepExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(TargetImage);
            
        // Correct brightness and saturation
        TargetImage.Mutate(imageContext =>
        {
            imageContext
                .Brightness(options.Brightness)
                .Saturate(options.Saturation);
        });

        return ExecutionResult.Next();
    }
}

internal static class ColourCorrectExtensions
{
    internal static IStepBuilder<TData, ColourCorrect> ColourCorrect<TStep, TData>(this IStepBuilder<TData, TStep> builder)
        where TStep : IStepBody
        where TData : WorkflowData
        => builder
            .Then<TStep, ColourCorrect, TData>("Colour correct")
            .Input(step => step.TargetImage, data => data.TargetImage);
}