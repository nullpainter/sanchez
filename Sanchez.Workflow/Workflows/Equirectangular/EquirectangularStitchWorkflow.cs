using System.Linq;
using JetBrains.Annotations;
using Sanchez.Processing.Models;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models;
using Sanchez.Workflow.Models.Data;
using Sanchez.Workflow.Steps.Common;
using Sanchez.Workflow.Steps.Equirectangular.Stitch;
using WorkflowCore.Interface;

namespace Sanchez.Workflow.Workflows.Equirectangular
{
    [UsedImplicitly]
    public class EquirectangularStitchWorkflow : IWorkflow<StitchWorkflowData>
    {
        private readonly RenderOptions _options;

        public EquirectangularStitchWorkflow(RenderOptions options) => _options = options;

        public void Build(IWorkflowBuilder<StitchWorkflowData> builder)
        {
            builder
                .Initialise()
                .CreateActivity()
                .InitialiseProgressBar(data => data.Activity!.Registrations.Count + 1)
                .If(data => data.Activity!.Registrations.Any())
                .Do(branch => branch
                    .ShouldWrite(_options.Timestamp)
                    .Branch(true, builder
                        .CreateBranch()
                        .CalculateVisibleRange()
                        .CalculateGlobalOffset()
                        .ForEach(data => data.Activity!.Registrations, _ => false)
                        .Do(registration => registration
                            .SetWorkflowRegistration()
                            .LoadImage(data => data.ProgressBar)
                            .NormaliseImage()
                            .ToEquirectangular()
                        )
                        .StitchImages()
                        .GetCropBounds()
                        .RenderUnderlay()
                        .ColourCorrect()
                        .CropImage()
                        .SaveStitchedImage(data => data.ProgressBar)
                    )
                )
                .LogCompletion();
        }

        public string Id => WorkflowConstants.EquirectangularBatch;
        public int Version => 1;
    }
}