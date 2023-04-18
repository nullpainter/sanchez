using JetBrains.Annotations;
using ShellProgressBar;

namespace Sanchez.Workflow.Models.Data;

public record GeostationaryTimelapseWorkflowData : TimelapseWorkflowData
{
    public double? Longitude { get; init; } 
}
    
/// <summary>
///     Data backing workflows which perform timelapse animations.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public record TimelapseWorkflowData : StitchWorkflowData
{
    internal List<DateTime> TimeIntervals { get; init;  } = new();
        
    /// <summary>
    ///     Timestamp of currently-processed step.
    /// </summary>
    public DateTime? TargetTimestamp { get; set; }
        
    /// <summary>
    ///     Progress bar rendering individual image rendering steps.
    /// </summary>
    public IProgressBar? ImageProgressBar { get; init; }

    public override void Dispose()
    {
        base.Dispose();
        ImageProgressBar?.Dispose();
        GC.SuppressFinalize(this);
    }
}