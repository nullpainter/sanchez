using Microsoft.Extensions.Logging;
using Sanchez.Processing.Extensions.Images;
using Sanchez.Processing.ImageProcessing.Crop;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Projections;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using Sanchez.Workflow.Models.Steps;
using SixLabors.ImageSharp.Processing;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Common;

internal sealed class NormaliseImage : StepBody, IRegistrationStepBody
{
    private readonly ILogger<NormaliseImage> _logger;
    private readonly RenderOptions _options;

    public NormaliseImage(ILogger<NormaliseImage> logger, RenderOptions options)
    {
        _logger = logger;
        _options = options;
    }

    public Registration? Registration { get; set; }

    public override ExecutionResult Run(IStepExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(Registration);

        NormaliseEwsCrop();

        Registration.Normalise(_options);
        return ExecutionResult.Next();
    }

    /// <summary>
    ///     EWS-G1 images either require cropping on the left edge or the right edge, depending on the time of day. The
    ///     <c>Satellites.json</c> file contains crop values for a right-hand crop. This method identifies if a left
    ///     crop is required and adjusts the crop bounds accordingly.
    /// </summary>
    private void NormaliseEwsCrop()
    {
        ArgumentNullException.ThrowIfNull(Registration?.Image);
        if (Registration.Definition.DisplayName != "EWS-G1-GOES13" || Registration.Definition.Crop == null) return;
            
        var operation = new EwsAlignmentRowOperation();
        Registration.Image.Mutate(c => c.ProcessPixelRowsAsVector4(row => operation.Invoke(row)));

        if (operation.IsRightCrop(Registration.Image)) return;
            
        _logger.LogInformation("Swapping horizontal crop bounds for EWS-G1");
        Registration.FlipHorizontalCrop = true;
    }
}

internal static class NormaliseImageExtensions
{
    internal static IStepBuilder<TData, NormaliseImage> NormaliseImage<TData, TStep>(this IStepBuilder<TData, TStep> builder)
        where TStep : IStepBody
        where TData : WorkflowData
    {
        return builder
            .Then<TStep, NormaliseImage, TData>("Normalise image")
            .WithRegistration();
    }
}