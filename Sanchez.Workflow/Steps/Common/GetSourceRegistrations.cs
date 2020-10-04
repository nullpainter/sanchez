using System.Collections.Generic;
using System.IO;
using Sanchez.Processing.Models.Projections;
using Sanchez.Processing.Services;
using Sanchez.Shared.Exceptions;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Common
{
    public class GetSourceRegistrations : StepBody
    {
        private readonly IFileService _fileService;

        public GetSourceRegistrations(IFileService fileService) => _fileService = fileService;
        public List<Registration>? SourceRegistrations { get; private set; }

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            try
            {
                var sourceFiles = _fileService.GetSourceFiles();
                SourceRegistrations = _fileService.ToRegistrations(sourceFiles);
            }
            catch (DirectoryNotFoundException e)
            {
                throw new ValidationException("Source directory not found", e);
            }
            
            return ExecutionResult.Next();
        }
    }
    
    internal static class GetSourceFilesExtensions
    {
        internal static IStepBuilder<TData, GetSourceRegistrations> GetSourceRegistrations<TStep, TData>(this IStepBuilder<TData, TStep> builder)
            where TStep : IStepBody
            where TData : WorkflowData
            => builder
                .Then<TStep, GetSourceRegistrations, TData>()
                .Output(data => data.SourceRegistrations, step => step.SourceRegistrations);
    }
}