using System.Linq;
using JetBrains.Annotations;
using Sanchez.Processing.Models;
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
        private readonly RenderOptions _options;

        public EquirectangularWorkflow(RenderOptions options) => _options = options;

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
                        .CalculateVisibleRange()
                        .CalculateGlobalOffset()
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
                                .GetCropBounds()
                                .RenderUnderlay()
                                .ColourCorrect()
                                .If(_ => _options.OverlayPath != null)
                                .Do(overlayBranch => overlayBranch
                                    .LoadOverlay()
                                    .ToEquirectangularOverlay()
                                    .RenderOverlay()
                                )
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