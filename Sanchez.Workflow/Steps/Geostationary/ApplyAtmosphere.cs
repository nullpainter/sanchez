using JetBrains.Annotations;
using Sanchez.Processing.Helpers;
using Sanchez.Processing.ImageProcessing.Atmosphere;
using Sanchez.Processing.Models;
using Sanchez.Processing.Services;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Geostationary;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class ApplyAtmosphere : StepBody
{
    private readonly RenderOptions _options;
    private readonly IGradientService _gradientService;

    public ApplyAtmosphere(RenderOptions options, IGradientService gradientService)
    {
        _options = options;
        _gradientService = gradientService;
    }

    public Image<Rgba32>? TargetImage { get; set; }

    public override ExecutionResult Run(IStepExecutionContext context)
    {
        if (!_options.HasAtmosphere) return ExecutionResult.Next();

        ArgumentNullException.ThrowIfNull(TargetImage);
        ArgumentNullException.ThrowIfNull(_options.GeostationaryRender);

        // Only apply atmosphere with underlay
        if (_options.NoUnderlay) return ExecutionResult.Next();

        var atmosphereAmount = _options.GeostationaryRender.AtmosphereAmount;
        var atmosphereOpacity = _options.GeostationaryRender.AtmosphereOpacity;

        var gradient = _gradientService.GetGradient(PathHelper.ResourcePath("Gradients/Atmosphere.json"));
        
        // Increase size of image to allow for atmosphere rendering
        var resizeOptions = new ResizeOptions
        {
            Size = new Size(
                (int)Math.Round(TargetImage.Width * AtmosphereRowOperation.ImageScaleFactor),
                (int)Math.Round(TargetImage.Height * AtmosphereRowOperation.ImageScaleFactor)
            ),
            Mode = ResizeMode.BoxPad,
            PadColor = Color.Black
        };

        TargetImage.Mutate(c => c.Resize(resizeOptions));
        TargetImage.ApplyAtmosphere(atmosphereAmount, atmosphereOpacity, gradient);

        return ExecutionResult.Next();
    }
}

internal static class ApplyAtmosphereExtensions
{
    internal static IStepBuilder<TData, ApplyAtmosphere> ApplyAtmosphere<TStep, TData>(this IStepBuilder<TData, TStep> builder)
        where TStep : IStepBody
        where TData : WorkflowData
        => builder
            .Then<TStep, ApplyAtmosphere, TData>("Apply atmosphere")
            .Input(step => step.TargetImage, data => data.TargetImage);
}