using System;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Sanchez.Processing.ImageProcessing.Tint;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Projections;
using Sanchez.Processing.Services.Underlay;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Geostationary
{
    internal sealed class RenderUnderlay : StepBodyAsync, IRegistrationStepBody
    {
        private readonly ILogger<RenderUnderlay> _logger;
        private readonly IUnderlayService _underlayService;
        private readonly RenderOptions _options;
        
        public Registration? Registration { get; set; }
        internal Image<Rgba32>? TargetImage { get; private set; }

        public RenderUnderlay(
            ILogger<RenderUnderlay> logger,
            IUnderlayService underlayService, 
            RenderOptions options)
        {
            _logger = logger;
            _underlayService = underlayService;
            _options = options;
        }

        public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            Guard.Against.Null(_options.GeostationaryRender, nameof(_options.GeostationaryRender));
            Guard.Against.Null(Registration?.Image, nameof(Registration.Image));
            
            var targetLongitude = _options.GeostationaryRender.Longitude;
            if (targetLongitude != null) throw new InvalidOperationException("Equirectangular composition should be used used when target longitude is provided");

            // Get or generate projected underlay
            var underlayOptions = new UnderlayProjectionOptions(
                ProjectionType.Geostationary,
                _options.InterpolationType,
                _options.ImageSize,
                _options.UnderlayPath);

            _logger.LogInformation("Retrieving underlay");
            var underlay = await _underlayService.GetUnderlayAsync(underlayOptions, Registration.Definition);

            _logger.LogInformation("Tinting and normalising IR imagery");
            if (_options.AutoAdjustLevels) Registration.Image.Mutate(c => c.HistogramEqualization());

            TargetImage = Registration.Image.Clone();
            TargetImage.Tint(_options.Tint);

            _logger.LogInformation("Blending with underlay");
            TargetImage.Mutate(c => c
                .Resize(_options.ImageSize, _options.ImageSize)
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
}