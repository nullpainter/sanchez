using System.IO;
using Ardalis.GuardClauses;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Sanchez.Processing.Filesystem;
using Sanchez.Processing.Models.Projections;
using Sanchez.Processing.Services;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models;
using ShellProgressBar;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Common
{
    public class ShouldWriteSingle : StepBody, IRegistrationStepBody, IProgressBarStepBody
    {
        private readonly ILogger<ShouldWriteSingle> _logger;
        private readonly SingleFilenameProvider _filenameProvider;
        private readonly IFileService _fileService;

        public ShouldWriteSingle(
            ILogger<ShouldWriteSingle> logger,
            SingleFilenameProvider filenameProvider,
            IFileService fileService)
        {
            _logger = logger;
            _filenameProvider = filenameProvider;
            _logger = logger;
            _fileService = fileService;
        }

        public Registration? Registration { get; set; }
        public int AlreadyRenderedCount { get; [UsedImplicitly] set; }
        public IProgressBar? ProgressBar { get; set; }

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            Guard.Against.Null(Registration, nameof(Registration));
            Guard.Against.Null(ProgressBar, nameof(ProgressBar));

            // Verify that the output file can be written
            Registration.OutputPath = _filenameProvider.GetOutputFilename(Registration.Path);
            if (_fileService.ShouldWrite(Registration.OutputPath))
            {
                ProgressBar.Tick($"Rendering {Path.GetFileName(Registration.Path)}");
                return ExecutionResult.Outcome(true);
            }

            _logger.LogInformation("Output file {outputFilename} exists; not overwriting", Registration.OutputPath);

            AlreadyRenderedCount++;
            ProgressBar.Tick($"Skipping {Path.GetFileName(Registration.Path)}");

            return ExecutionResult.Outcome(false);
        }
    }

    internal static class ShouldWriteExtensions
    {
        internal static IStepBuilder<TData, ShouldWriteSingle> ShouldWriteSingle<TStep, TData>(this IStepBuilder<TData, TStep> builder)
            where TStep : IStepBody
            where TData : WorkflowData
            => builder
                .Then<TStep, ShouldWriteSingle, TData>()
                .WithRegistration()
                .WithProgressBar()
                .Input(step => step.AlreadyRenderedCount, data => data.AlreadyRenderedCount)
                .Output(data => data.AlreadyRenderedCount, step => step.AlreadyRenderedCount);
    }
}