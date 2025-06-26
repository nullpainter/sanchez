﻿using JetBrains.Annotations;
using Sanchez.Processing.Models;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models;
using Sanchez.Workflow.Models.Data;
using Sanchez.Workflow.Steps.Common;
using Sanchez.Workflow.Steps.Equirectangular;
using Sanchez.Workflow.Steps.Equirectangular.Stitch;
using Sanchez.Workflow.Steps.Geostationary;
using Sanchez.Workflow.Steps.Geostationary.Reprojected;
using WorkflowCore.Interface;

namespace Sanchez.Workflow.Workflows.Geostationary;

[UsedImplicitly]
internal class GeostationaryReprojectedWorkflow(RenderOptions options) : IWorkflow<StitchWorkflowData>
{
    public void Build(IWorkflowBuilder<StitchWorkflowData> builder)
    {
        builder
            .Initialise()
            .CreateActivity()
            .InitialiseProgressBar(data => data.Activity!.Registrations.Count + 2)
            .ShouldWrite(options.Timestamp)
            .Branch(true, builder.CreateBranch()
                .GetVisibleRange()
                .GetGlobalOffset()
                .ForEach(data => data.Activity!.Registrations, _ => false)
                .Do(registration => registration
                    .SetWorkflowRegistration()
                    .LoadImage(data => data.ProgressBar)
                    .NormaliseImage()
                    .ToEquirectangular()
                )
                .StitchImages()
                .RenderOverlay(d => d.TargetImage)
                .RenderUnderlay()
                .ToGeostationary(options1 => options.GeostationaryRender!.Longitude)
                .If(data => data.OverlayImage != null).Do(step => step.ToGeostationary(options1 => options.GeostationaryRender!.Longitude, data => data.OverlayImage))
                .ComposeOverlay()
                .ColourCorrect()
                .ApplyAtmosphere()
                .ApplyNoise()
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