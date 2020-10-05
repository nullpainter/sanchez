using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Projections;
using ShellProgressBar;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Sanchez.Workflow.Models.Data
{
    public interface IWorkflowData
    {
    }

    /// <summary>
    ///     Base class for data shared between steps.
    /// </summary>
    /// <remarks>
    ///    This is arguably a misuse of WorkflowCore data, as it should be able to be serialized. Add disposable
    ///    services and binary data breaks this pattern. In practice, this state needs to be available somewhere,
    ///    and the workflows aren't log enough to warrant having a persistence provider specified.
    /// </remarks>
    public abstract class WorkflowData : IWorkflowData, IDisposable
    {
        /// <summary>
        ///     Number of images which have been skipped as they have already been rendered.
        /// </summary>
        public int AlreadyRenderedCount { get; [UsedImplicitly] set; }

        /// <summary>
        ///     Number of images that have been rendered by the current workflow.
        /// </summary>
        public int RenderedCount { get; [UsedImplicitly] set; }
        
        /// <summary>
        ///     All source images and associated satellite definitions matching the source glob,
        ///     unfiltered by timestamp or eligibility.
        /// </summary>
        public List<Registration>? SourceRegistrations { get; [UsedImplicitly] set; }

        /// <summary>
        ///     Target image being composed.
        /// </summary>
        public Image<Rgba32>? TargetImage { get; [UsedImplicitly] set; }

        /// <summary>
        ///     Currently processed activity.
        /// </summary>
        public Activity? Activity { get; set; }

        /// <summary>
        ///     Currently processed registration.
        /// </summary>
        public Registration? Registration { get; set; }

        public IProgressBar? ProgressBar { get; [UsedImplicitly] set; }

        public virtual void Dispose()
        {
            TargetImage?.Dispose();
            Activity?.Dispose();
            Registration?.Dispose();
            ProgressBar?.Dispose();
        }
    }
}