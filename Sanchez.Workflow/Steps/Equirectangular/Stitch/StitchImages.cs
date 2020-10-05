using System.Linq;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Projections;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models;
using Sanchez.Workflow.Models.Data;
using Sanchez.Workflow.Models.Steps;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Equirectangular.Stitch
{
    internal sealed class StitchImages : StepBody, IRegistrationStepBody, IActivityStepBody
    {
        private readonly ILogger<StitchImages> _logger;
        public Image<Rgba32>? TargetImage { get; set; }
        public Activity? Activity { get; set; }
        public Registration? Registration { get; set; }

        public StitchImages(ILogger<StitchImages> logger)
        {
            _logger = logger;
        }
        
        public override ExecutionResult Run(IStepExecutionContext context)
        {
            Guard.Against.Null(Activity, nameof(Activity));

            // Identify minimum horizontal offset im source images
            var minOffset = Activity.Registrations.Select(p => p.OffsetX).Min();
            TargetImage = NewTargetImage(minOffset);

            _logger.LogInformation("Output image size: {width} x {height} px", TargetImage.Width, TargetImage.Height);

            // Composite all images. Images will have their horizontal offsets pre-calculated and overlaps
            // blended, so compositing just involves combining them in the correct stacking order.
            TargetImage.Mutate(imageContext =>
            {
                // Render all images in correct stacking order
                foreach (var registration in Activity.Registrations.OrderByDescending(p => p.OffsetX))
                {
                    using (registration)
                    {
                        var projectionImage = registration.Image;

                        // Identify horizontal offset of each image
                        var location = new Point(registration.OffsetX - minOffset, 0);
                        imageContext.DrawImage(projectionImage, location, PixelColorBlendingMode.Normal, 1.0f);
                    }
                }
            });
            
            return ExecutionResult.Next();
        }

        /// <summary>
        ///     Initialises the target image, calculating image size based on size of source images and
        ///     adjusting for image offsets.
        /// </summary>
        private Image<Rgba32> NewTargetImage(int minOffset)
        {
            // As we know the horizontal offsets of all images being composed, the output width is the 
            // maximum offset plus the width of the final image, minus the minimum offset.
            var finalProjection = Activity!.Registrations.OrderBy(p => p.OffsetX).Last();

            var outputWidth = finalProjection.OffsetX + finalProjection.Width - minOffset;
            return new Image<Rgba32>(outputWidth, finalProjection.Height);
        }
    }

    internal static class StitchImagesExtensions
    {
        internal static IStepBuilder<TData, StitchImages> StitchImages<TStep, TData>(this IStepBuilder<TData, TStep> builder)
            where TStep : IStepBody
            where TData : EquirectangularStitchWorkflowData
        {
            return builder
                .Then<TStep, StitchImages, TData>("Stitch images")
                .WithRegistration()
                .WithActivity()
                .Output(data => data.TargetImage, step => step.TargetImage);
        }
    }
}