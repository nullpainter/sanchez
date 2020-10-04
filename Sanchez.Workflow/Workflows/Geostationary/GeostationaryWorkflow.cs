using JetBrains.Annotations;
using Sanchez.Workflow.Models;
using Sanchez.Workflow.Steps.Common;
using Sanchez.Workflow.Steps.Geostationary;
using WorkflowCore.Interface;

namespace Sanchez.Workflow.Workflows.Geostationary
{
    [UsedImplicitly]
    internal class GeostationaryWorkflow : IWorkflow<GeostationaryWorkflowData>
    {
        public void Build(IWorkflowBuilder<GeostationaryWorkflowData> builder)
        {
            builder
                .InitialiseUnderlayCache()
                .InitialiseSatelliteRegistry()
                .GetSourceRegistrations()
                .CreateActivity()
                .InitialiseProgressBar(data => data.Activity!.Registrations.Count + 1)
                .ForEach(data => data.Activity!.Registrations, _ => false)
                .Do(registration => registration
                    .SetWorkflowRegistration()
                    .ShouldWriteSingle()
                    .Branch(true, builder.CreateBranch()
                        .LoadImageSingle()
                        .NormaliseImage()
                        .RenderUnderlay()
                        .ColourCorrect()
                        .ApplyHaze()
                        .SaveImage()
                    )
                )
                .LogCompletion();
        }

        public string Id => WorkflowConstants.Geostationary;
        public int Version => 1;
    }
}