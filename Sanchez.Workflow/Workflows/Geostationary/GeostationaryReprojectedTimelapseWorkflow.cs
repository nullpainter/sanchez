using System.Linq;
using JetBrains.Annotations;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models;
using Sanchez.Workflow.Models.Data;
using Sanchez.Workflow.Steps.Common;
using Sanchez.Workflow.Steps.Equirectangular.Stitch;
using Sanchez.Workflow.Steps.Equirectangular.Timelapse;
using Sanchez.Workflow.Steps.Geostationary;
using Sanchez.Workflow.Steps.Geostationary.Reprojected;
using WorkflowCore.Interface;

namespace Sanchez.Workflow.Workflows.Geostationary
{
    [UsedImplicitly]
    public class GeostationaryReprojectedTimelapseWorkflow : IWorkflow<TimelapseWorkflowData>
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
                    .Do(timeStep => timeStep
                        .SetTargetTimestamp()
                        .CreateActivities()
                        .ShouldWrite()
                        .Branch(true, timeStep
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
                            .RenderUnderlay()
                            .ColourCorrect()
                            .ToGeostationary()
                            .ApplyHaze()
                            .SaveStitchedImage(data => data.ProgressBar)
                        )
                    )
                )
                .LogCompletion();
        }

        public string Id => WorkflowConstants.GeostationaryReprojectedTimelapse;
        public int Version => 1;
    }
}