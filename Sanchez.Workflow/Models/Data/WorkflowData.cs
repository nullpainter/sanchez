using JetBrains.Annotations;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Projections;
using ShellProgressBar;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Sanchez.Workflow.Models.Data;

public interface IWorkflowData
{
}

/// <summary>
///     Base class for data shared between steps.
/// </summary>
/// <remarks>
///    This is arguably a misuse of WorkflowCore data, as it should be able to be serialized. Adding disposable
///    services and binary data breaks this pattern. In practice, this state needs to be available somewhere,
///    and the workflows aren't log enough to warrant having a persistence provider specified.
/// </remarks>
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public abstract record WorkflowData : IWorkflowData, IDisposable
{
    /// <summary>
    ///     Number of images which have been skipped as they have already been rendered.
    /// </summary>
    public int AlreadyRenderedCount { get; init; }

    /// <summary>
    ///     Number of images that have been rendered by the current workflow.
    /// </summary>
    public int RenderedCount { get; init; }
        
    /// <summary>
    ///     All source images and associated satellite definitions matching the source glob,
    ///     unfiltered by timestamp or eligibility.
    /// </summary>
    public List<Registration>? SourceRegistrations { get; init; }

    /// <summary>
    ///     Target image being composed.
    /// </summary>
    public Image<Rgba32>? TargetImage { get; init; }
        
    /// <summary>
    ///     Optional overlay image, containing IR enhanced with a custom colour palette.
    /// </summary>
    public Image<Rgba32>? OverlayImage { get; init; }
        
    /// <summary>
    ///     Currently processed activity.
    /// </summary>
    public Activity? Activity { get; set; }

    /// <summary>
    ///     Currently processed registration.
    /// </summary>
    public Registration? Registration { get; set; }

    public IProgressBar? ProgressBar { get; init; }

    public virtual void Dispose()
    {
        TargetImage?.Dispose();
        Activity?.Dispose();
        Registration?.Dispose();
        ProgressBar?.Dispose();
        
        GC.SuppressFinalize(this);
    }
}