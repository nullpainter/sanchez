using System.Linq;
using JetBrains.Annotations;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models;
using Sanchez.Workflow.Models.Data;
using Sanchez.Workflow.Steps.Common;
using Sanchez.Workflow.Steps.Equirectangular.Stitch;
using Sanchez.Workflow.Steps.Equirectangular.Timelapse;
using WorkflowCore.Interface;

namespace Sanchez.Workflow.Workflows.Equirectangular
{
    [UsedImplicitly]
    public class EquirectangularTimelapseWorkflow : IWorkflow<TimelapseWorkflowData>
    {
        public void Build(IWorkflowBuilder<TimelapseWorkflowData> builder)
        {
            builder
                .Initialise()
                .PrepareTimeIntervals()
                .InitialiseProgressBar(data => data.TimeIntervals.Count)
                .If(data => data.TimeIntervals.Any())
                .Do(branch => branch
                    .ForEach(data => data.TimeIntervals, options => false)
                    .Do(step => step
                        .SetTargetTimestamp()
                        .CreateActivities()
                        .ShouldWrite()
                        .Branch(true, step
                            .CreateBranch()
                            .InitialiseImageProgressBar(data => data.Activity!.Registrations.Count + 1)
                            .CalculateVisibleRange()
                            .CalculateGlobalOffset()
                            .ForEach(data => data.Activity!.Registrations, _ => false)
                            .Do(registration => registration
                                .SetWorkflowRegistration()
                                .LoadImage(data => data.ImageProgressBar)
                                .NormaliseImage()
                                .ToEquirectangular()
                            )
                            .StitchImages()
                            .EqualiseOverlayHistogram(data => data.TargetImage)
                            .RenderOverlay(data => data.TargetImage)
                            .GetCropBounds()
                            .RenderUnderlay()
                            .ComposeOverlay()
                            .ColourCorrect()
                            .CropImage()
                            .SaveStitchedImage(data => data.ImageProgressBar)
                        )
                    )
                )
                .LogCompletion();
        }

        public string Id => WorkflowConstants.EquirectangularTimelapse;
        public int Version => 1;
    }
}