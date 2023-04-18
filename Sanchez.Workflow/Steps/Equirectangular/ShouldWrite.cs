using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Sanchez.Processing.Filesystem.Equirectangular;
using Sanchez.Processing.Models;
using Sanchez.Processing.Services;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using Sanchez.Workflow.Models.Steps;
using ShellProgressBar;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Equirectangular;

public class ShouldWrite : StepBody, IActivityStepBody, IProgressBarStepBody
{
    private readonly RenderOptions _options;
    private readonly StitchedFilenameProvider _filenameProvider;
    private readonly ILogger<ShouldWrite> _logger;
    private readonly IFileService _fileService;

    public ShouldWrite(
        RenderOptions options,
        StitchedFilenameProvider filenameProvider,
        ILogger<ShouldWrite> logger,
        IFileService fileService)
    {
        _options = options;
        _filenameProvider = filenameProvider;
        _logger = logger;
        _logger = logger;
        _fileService = fileService;
    }

    public IProgressBar? ProgressBar { get; set; }
    public Activity? Activity { get; set; }
    public DateTime? Timestamp { get; set; }
    public int AlreadyRenderedCount { get; [UsedImplicitly] set; }
    public string? Identifier { get; [UsedImplicitly] set; }

    public override ExecutionResult Run(IStepExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(Activity);
        ArgumentNullException.ThrowIfNull(Timestamp);
        ArgumentNullException.ThrowIfNull(ProgressBar);

        Activity.OutputPath = _filenameProvider.GetOutputFilename(Timestamp.Value);

        if (!Activity.Registrations.Any())
        {
            _logger.LogInformation("No images found; skipping");

            ProgressBar.Tick($"Scanning {Timestamp:s}{Identifier}");
            return ExecutionResult.Outcome(false);
        }

        // Verify minimum number of satellites
        if (_options.MinSatellites != null && Activity.Registrations.Count < _options.MinSatellites)
        {
            _logger.LogInformation("Fewer than {MinSatellites} for {Timestamp}; skipping", _options.MinSatellites, Timestamp);

            ProgressBar.Tick($"Skipping {Timestamp:s}");
            return ExecutionResult.Outcome(false);
        }

        // Verify that the output file can be written
        if (_fileService.ShouldWrite(Activity.OutputPath))
        {
            ProgressBar.Tick($"Processing {Timestamp:s}{Identifier}");
            return ExecutionResult.Outcome(true);
        }

        _logger.LogInformation("Output file {OutputFilename} exists; not overwriting", Activity.OutputPath);
        AlreadyRenderedCount++;

        ProgressBar.Tick($"Skipping {Timestamp:s}{Identifier}");

        return ExecutionResult.Outcome(false);
    }
}

internal static class ShouldWriteExtensions
{
    internal static IStepBuilder<TData, ShouldWrite> ShouldWrite<TData>(
        this IWorkflowBuilder<TData> builder,
        DateTime? timestamp,
        Expression<Func<TData, string?>>? identifier = null)
        where TData : WorkflowData
    {
        var result = builder
            .StartWith<ShouldWrite, TData>()
            .WithActivity()
            .WithProgressBar()
            .Input(step => step.Timestamp, data => timestamp)
            .Output(data => data.AlreadyRenderedCount, step => step.AlreadyRenderedCount);

        if (identifier != null) result.Input(step => step.Identifier, identifier);

        return result;
    }

    internal static IStepBuilder<TData, ShouldWrite> ShouldWrite<TStep, TData>(
        this IStepBuilder<TData, TStep> builder,
        DateTime? timestamp,
        Expression<Func<TData, string?>>? identifier = null)
        where TStep : IStepBody
        where TData : WorkflowData
    {
        var result = builder
            .Then<TStep, ShouldWrite, TData>()
            .WithActivity()
            .WithProgressBar()
            .Input(step => step.Timestamp, data => timestamp)
            .Output(data => data.AlreadyRenderedCount, step => step.AlreadyRenderedCount);

        if (identifier != null) result.Input(step => step.Identifier, identifier);

        return result;
    }

    internal static IStepBuilder<TData, ShouldWrite> ShouldWrite<TStep, TData>(
        this IStepBuilder<TData, TStep> builder,
        Expression<Func<TData, string?>>? identifier = null)
        where TStep : IStepBody
        where TData : TimelapseWorkflowData
    {
        var result = builder
            .Then<TStep, ShouldWrite, TData>()
            .WithActivity()
            .WithProgressBar()
            .Input(step => step.Timestamp, data => data.TargetTimestamp)
            .Output(data => data.AlreadyRenderedCount, step => step.AlreadyRenderedCount);

        if (identifier != null) result.Input(step => step.Identifier, identifier);

        return result;
    }
}