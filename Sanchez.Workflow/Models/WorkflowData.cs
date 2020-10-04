using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Projections;
using ShellProgressBar;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Sanchez.Workflow.Models
{
    public interface IWorkflowData
    {
    }

    public abstract class WorkflowData : IWorkflowData, IDisposable
    {
        /// <summary>
        ///     Number of images which have been skipped as they have already been rendered.
        /// </summary>
        public int AlreadyRenderedCount { get; [UsedImplicitly] set; }

        public int RenderedCount { get; [UsedImplicitly] set; }
        public List<Registration>? SourceRegistrations { get; [UsedImplicitly] set; }
        
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