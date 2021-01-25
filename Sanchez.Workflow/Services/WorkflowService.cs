using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
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
namespace Sanchez.Workflow.Services
{
    /// <summary>
    ///     Initialises the workflow engine and provides management for starting, stopping
    ///     and cancelling workflows.
    /// </summary>
    public interface IWorkflowService
    {
        void Initialise(CancellationTokenSource cancellationToken);
        Task StartAsync(CancellationTokenSource cancellationToken);
    }

    public sealed class WorkflowService : IWorkflowService, IDisposable
    {
        private readonly RenderOptions _options;
        private readonly IWorkflowHost _host;

        private readonly AutoResetEvent _resetEvent;
        private string? _workflowId;
        private bool _initialised;

        public WorkflowService(
            RenderOptions options,
            IWorkflowHost host)
        {
            _options = options;
            _host = host;
            _resetEvent = new AutoResetEvent(false);
        }

        public void Initialise(CancellationTokenSource cancellationToken)
        {
            if (_initialised) throw new InvalidOperationException("Workflow service is already initialised");

            RegisterWorkflows();

            _host.OnStepError += (workflow, _, exception) => OnStepError(exception, workflow);
            _host.OnLifeCycleEvent += evt => OnLifeCycleEvent(cancellationToken, evt);
            Console.CancelKeyPress += (_, e) => CancelKeyPress(cancellationToken, e);

            _initialised = true;
        }

        private void RegisterWorkflows()
        {
            _host.RegisterWorkflow<GeostationaryWorkflow, GeostationaryWorkflowData>();
            _host.RegisterWorkflow<GeostationaryReprojectedWorkflow, StitchWorkflowData>();
            _host.RegisterWorkflow<EquirectangularStitchWorkflow, StitchWorkflowData>();
            _host.RegisterWorkflow<EquirectangularTimelapseWorkflow, TimelapseWorkflowData>();
            _host.RegisterWorkflow<EquirectangularWorkflow, EquirectangularWorkflowData>();
            _host.RegisterWorkflow<GeostationaryReprojectedTimelapseWorkflow, GeostationaryTimelapseWorkflowData>();
        }

        /// <summary>
        ///     Explicitly handle ctrl+c to avoid writing corrupted files.
        /// </summary>
        private void CancelKeyPress(CancellationTokenSource cancellationToken, ConsoleCancelEventArgs args)
        {
            args.Cancel = true;

            _host.StopAsync(cancellationToken.Token).Wait();
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

            _host.TerminateWorkflow(evt.WorkflowInstanceId);
            DisposeData();

            _resetEvent.Set();
        }

        private WorkflowInstance GetWorkflow() => _host.PersistenceStore.GetWorkflowInstance(_workflowId).GetAwaiter().GetResult();

        private void OnStepError(Exception exception, WorkflowInstance workflow)
        {
            DisposeData();

            switch (exception)
            {
                case ValidationException validationException:
                    Console.WriteLine(validationException.Message);
                    Log.Warning("{ValidationMessage}", validationException.Message);
                    break;
                default:
                    if (!_options.Verbose) Console.WriteLine("Unhandled failure; check logs for details, or run again with verbose logging (-v / --verbose)");
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
            await _host.StartAsync(cancellationToken.Token);

            _workflowId = _options.Projection switch
            {
                // Geostationary timelapse with target longitude
                ProjectionType.Geostationary when _options.GeostationaryRender!.Longitude != null && _options.Interval != null
                    => await _host.StartWorkflow(WorkflowConstants.GeostationaryReprojectedTimelapse),

                // Geostationary with target longitude
                ProjectionType.Geostationary when _options.GeostationaryRender!.Longitude != null
                    => await _host.StartWorkflow(WorkflowConstants.GeostationaryReprojected),

                // Geostationary without target longitude
                ProjectionType.Geostationary
                    => await _host.StartWorkflow(WorkflowConstants.Geostationary),

                // Equirectangular stitched timelapse
                ProjectionType.Equirectangular when _options.StitchImages && _options.Interval != null
                    => await _host.StartWorkflow(WorkflowConstants.EquirectangularTimelapse),

                // Equirectangular stitched
                ProjectionType.Equirectangular when _options.StitchImages
                    => await _host.StartWorkflow(WorkflowConstants.EquirectangularBatch),

                // Equirectangular 
                ProjectionType.Equirectangular
                    => await _host.StartWorkflow(WorkflowConstants.Equirectangular),

                _ => throw new InvalidOperationException("Unhandled projection scenario")
            };

            // Wait for workflow to complete
            _resetEvent.WaitOne();
        }

        public void Dispose() => _resetEvent.Dispose();
    }
}