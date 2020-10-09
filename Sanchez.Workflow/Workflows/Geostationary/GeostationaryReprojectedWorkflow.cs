using JetBrains.Annotations;
using Sanchez.Processing.Models;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models;
using Sanchez.Workflow.Models.Data;
using Sanchez.Workflow.Steps.Common;
using Sanchez.Workflow.Steps.Equirectangular.Stitch;
using Sanchez.Workflow.Steps.Geostationary;
using Sanchez.Workflow.Steps.Geostationary.Reprojected;
using WorkflowCore.Interface;

namespace Sanchez.Workflow.Workflows.Geostationary
{
    [UsedImplicitly]
    internal class GeostationaryReprojectedWorkflow : IWorkflow<StitchWorkflowData>
    {
        private readonly RenderOptions _options;

        public GeostationaryReprojectedWorkflow(RenderOptions options) => _options = options;

        public void Build(IWorkflowBuilder<StitchWorkflowData> builder)
        {
            builder
                .Initialise()
                .CreateActivity()
                .InitialiseProgressBar(data => data.Activity!.Registrations.Count + 2)
                .ShouldWrite(_options.Timestamp)
                .Branch(true, builder.CreateBranch()
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
                    .RenderUnderlay()
                    .ColourCorrect()
                    .ToGeostationary(options => _options.GeostationaryRender!.Longitude)
                    .ApplyHaze()
                    .SaveStitchedImage(data => data.ProgressBar)
                    .LogCompletion()
                )
                .Branch(false, builder
                    .CreateBranch()
                    .LogCompletion()
                );
        }

        public string Id => WorkflowConstants.GeostationaryReprojected;
        public int Version => 1;
    }
}