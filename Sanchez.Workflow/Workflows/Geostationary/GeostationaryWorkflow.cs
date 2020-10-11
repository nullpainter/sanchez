using System.Linq;
using JetBrains.Annotations;
using Sanchez.Processing.Models;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models;
using Sanchez.Workflow.Models.Data;
using Sanchez.Workflow.Steps.Common;
using Sanchez.Workflow.Steps.Geostationary;
using WorkflowCore.Interface;

namespace Sanchez.Workflow.Workflows.Geostationary
{
    [UsedImplicitly]
    internal class GeostationaryWorkflow : IWorkflow<GeostationaryWorkflowData>
    {
        private readonly RenderOptions _options;

        public GeostationaryWorkflow(RenderOptions options) => _options = options;

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
                            .RenderUnderlay()
                            .ColourCorrect()
                            .If(_ => _options.OverlayPath != null)
                            .Do(overlayBranch => overlayBranch
                                .LoadOverlay()
                                .RenderOverlay()
                            )
                            .ApplyHaze()
                            .SaveImage()
                        )
                    )
                )
                .LogCompletion();
        }

        public string Id => WorkflowConstants.Geostationary;
        public int Version => 1;
    }
}