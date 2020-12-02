using System.Linq;
using JetBrains.Annotations;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models;
using Sanchez.Workflow.Models.Data;
using Sanchez.Workflow.Steps.Common;
using Sanchez.Workflow.Steps.Equirectangular;
using Sanchez.Workflow.Steps.Equirectangular.Stitch;
using WorkflowCore.Interface;

namespace Sanchez.Workflow.Workflows.Equirectangular
{
    [UsedImplicitly]
    public class EquirectangularWorkflow : IWorkflow<EquirectangularWorkflowData>
    {
        public void Build(IWorkflowBuilder<EquirectangularWorkflowData> builder)
        {
            builder
                .Initialise()
                .RegisterImages()
                .InitialiseProgressBar(data => data.Activities.Count)
                .If(data => data.Activities.Any())
                .Do(branch => branch
                    .ForEach(data => data.Activities, _ => false)
                    .Do(activityStep => activityStep
                        .SetWorkflowActivity()
                        .GetVisibleRange()
                        .GetGlobalOffset()
                        .ForEach(data => data.Activity!.Registrations, _ => false) // Single registration
                        .Do(registration => registration
                            .SetWorkflowRegistration()
                            .ShouldWriteSingle()
                            .Branch(true, builder
                                .CreateBranch()
                                .LoadImageSingle()
                                .NormaliseImage()
                                .ToEquirectangular()
                                .StitchImages()
                                .RenderOverlay(data => data.TargetImage)
                                .GetCropBounds()
                                .RenderUnderlay()
                                .ComposeOverlay()
                                .ColourCorrect()
                                .OffsetImage()
                                .CropImage()
                                .SaveImage()
                            )
                        )
                    )
                )
                .LogCompletion();
        }

        public string Id => WorkflowConstants.Equirectangular;
        public int Version => 1;
    }
}