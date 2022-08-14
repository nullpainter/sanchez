using JetBrains.Annotations;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models;
using Sanchez.Workflow.Models.Data;
using Sanchez.Workflow.Steps.Common;
using Sanchez.Workflow.Steps.Geostationary;
using WorkflowCore.Interface;

namespace Sanchez.Workflow.Workflows.Geostationary;

[UsedImplicitly]
internal class GeostationaryWorkflow : IWorkflow<GeostationaryWorkflowData>
{
    public void Build(IWorkflowBuilder<GeostationaryWorkflowData> builder)
    {
        builder
            .Initialise()
            .CreateActivity()
            .InitialiseProgressBar(data => data.Activity!.Registrations.Count + 1)
            .If(data => data.Activity!.Registrations.Any())
            .Do(branch => branch
                .ForEach(data => data.Activity!.Registrations, _ => false)
                .Do(registration => registration
                    .SetWorkflowRegistration()
                    .ShouldWriteSingle()
                    .Branch(true, builder.CreateBranch()
                        .LoadImageSingle()
                        .NormaliseImage()
                        .RenderOverlay()
                        .RenderUnderlay()
                        .ComposeOverlay()
                        .ColourCorrect()
                        .ApplyAtmosphere()
                        .ApplyNoise()
                        .SaveImage()
                    )
                )
            )
            .LogCompletion();
    }

    public string Id => WorkflowConstants.Geostationary;
    public int Version => 1;
}