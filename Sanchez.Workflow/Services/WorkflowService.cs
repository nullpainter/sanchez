using System.Runtime.CompilerServices;
using FluentValidation;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Projections;
using Sanchez.Workflow.Models;
using Sanchez.Workflow.Models.Data;
using Sanchez.Workflow.Workflows.Equirectangular;
using Sanchez.Workflow.Workflows.Geostationary;
using Serilog;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using WorkflowCore.Models.LifeCycleEvents;

[assembly: InternalsVisibleTo("Sanchez.Workflow.Test")]
namespace Sanchez.Workflow.Services;

/// <summary>
///     Initialises the workflow engine and provides management for starting, stopping
///     and cancelling workflows.
/// </summary>
public interface IWorkflowService
{
    void Initialise(CancellationTokenSource cancellationToken);
    Task StartAsync(CancellationTokenSource cancellationToken);
}

public sealed class WorkflowService(
    RenderOptions options,
    IWorkflowHost host) : IWorkflowService, IDisposable
{
    private readonly AutoResetEvent _resetEvent = new(false);
    private string? _workflowId;
    private bool _initialised;

    public void Initialise(CancellationTokenSource cancellationToken)
    {
        if (_initialised) throw new InvalidOperationException("Workflow service is already initialised");

        RegisterWorkflows();

        host.OnStepError += (workflow, _, exception) => OnStepError(exception, workflow);
        host.OnLifeCycleEvent += evt => OnLifeCycleEvent(cancellationToken, evt);
        Console.CancelKeyPress += (_, e) => CancelKeyPress(cancellationToken, e);

        _initialised = true;
    }

    private void RegisterWorkflows()
    {
        host.RegisterWorkflow<GeostationaryWorkflow, GeostationaryWorkflowData>();
        host.RegisterWorkflow<GeostationaryReprojectedWorkflow, StitchWorkflowData>();
        host.RegisterWorkflow<EquirectangularStitchWorkflow, StitchWorkflowData>();
        host.RegisterWorkflow<EquirectangularTimelapseWorkflow, TimelapseWorkflowData>();
        host.RegisterWorkflow<EquirectangularWorkflow, EquirectangularWorkflowData>();
        host.RegisterWorkflow<GeostationaryReprojectedTimelapseWorkflow, GeostationaryTimelapseWorkflowData>();
    }

    /// <summary>
    ///     Explicitly handle ctrl+c to avoid writing corrupted files.
    /// </summary>
    private void CancelKeyPress(CancellationTokenSource cancellationToken, ConsoleCancelEventArgs args)
    {
        args.Cancel = true;

        host.StopAsync(cancellationToken.Token).Wait();
        DisposeData();

        _resetEvent.Set();
    }

    private void DisposeData()
    {
        var data = (WorkflowData) GetWorkflow().Data;
        data.Dispose();
    }

    private void OnLifeCycleEvent(CancellationTokenSource cancellationToken, LifeCycleEvent evt)
    {
        // Dispose data and signal workflow completed
        if (evt is WorkflowCompleted)
        {
            DisposeData();
            _resetEvent.Set();

            return;
        }

        // Ignore other lifecycle events which aren't related to user cancellation
        if (!cancellationToken.IsCancellationRequested) return;

        var data = (WorkflowData) GetWorkflow().Data;
        if (data.ProgressBar != null) data.ProgressBar.Message = $"Cancelled after rendering {data.RenderedCount} images";

        host.TerminateWorkflow(evt.WorkflowInstanceId).GetAwaiter().GetResult();
        DisposeData();

        _resetEvent.Set();
    }

    private WorkflowInstance GetWorkflow() => host.PersistenceStore.GetWorkflowInstance(_workflowId).GetAwaiter().GetResult();

    private void OnStepError(Exception exception, WorkflowInstance workflow)
    {
        DisposeData();
        
        // Silently ignore task cancellation exceptions as this is likely caused by user-initialised cancellation
        if (exception.GetBaseException().GetType() == typeof(TaskCanceledException)) return;

        switch (exception)
        {
            case ValidationException validationException:
                Console.WriteLine(validationException.Message);
                Log.Warning("{ValidationMessage}", validationException.Message);
                break;
            default:
                if (!options.Verbose) Console.WriteLine("Unhandled failure; check logs for details, or run again with verbose logging (-v / --verbose)");
                Log.Error(exception, "Unhandled failure in workflow");
                break;
        }

        // Prevent workflow from retrying
        workflow.Status = WorkflowStatus.Terminated;
        _resetEvent.Set();
    }

    public async Task StartAsync(CancellationTokenSource cancellationToken)
    {
        if (!_initialised) throw new InvalidOperationException($"Call {nameof(Initialise)}() before starting a workflow.");

        // Start workflow host
        await host.StartAsync(cancellationToken.Token);

        _workflowId = options.Projection switch
        {
            // Geostationary timelapse with target longitude
            ProjectionType.Geostationary when options.GeostationaryRender!.Longitude != null && options.Interval != null
                => await host.StartWorkflow(WorkflowConstants.GeostationaryReprojectedTimelapse),

            // Geostationary with target longitude
            ProjectionType.Geostationary when options.GeostationaryRender!.Longitude != null
                => await host.StartWorkflow(WorkflowConstants.GeostationaryReprojected),

            // Geostationary without target longitude
            ProjectionType.Geostationary
                => await host.StartWorkflow(WorkflowConstants.Geostationary),

            // Equirectangular stitched timelapse
            ProjectionType.Equirectangular when options is { StitchImages: true, Interval: not null }
                => await host.StartWorkflow(WorkflowConstants.EquirectangularTimelapse),

            // Equirectangular stitched
            ProjectionType.Equirectangular when options.StitchImages
                => await host.StartWorkflow(WorkflowConstants.EquirectangularBatch),

            // Equirectangular 
            ProjectionType.Equirectangular
                => await host.StartWorkflow(WorkflowConstants.Equirectangular),

            _ => throw new InvalidOperationException("Unhandled projection scenario")
        };

        // Wait for workflow to complete
        _resetEvent.WaitOne();
    }

    public void Dispose() => _resetEvent.Dispose();
}