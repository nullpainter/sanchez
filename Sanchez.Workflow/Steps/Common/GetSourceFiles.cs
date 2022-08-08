using FluentValidation;
using Sanchez.Processing.Models.Projections;
using Sanchez.Processing.Services;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Common;

public class GetSourceFiles : StepBody
{
    private readonly IFileService _fileService;

    public GetSourceFiles(IFileService fileService) => _fileService = fileService;
    public List<Registration>? SourceRegistrations { get; private set; }

    public override ExecutionResult Run(IStepExecutionContext context)
    {
        try
        {
            var sourceFiles = _fileService.GetSourceFiles();
            if (!sourceFiles.Any()) throw new ValidationException("No source files found");
                
            SourceRegistrations = _fileService.ToRegistrations(sourceFiles, context.CancellationToken);
        }
        catch (DirectoryNotFoundException)
        {
            throw new ValidationException("Source directory not found");
        }
            
        return ExecutionResult.Next();
    }
}
    
internal static class GetSourceFilesExtensions
{
    internal static IStepBuilder<TData, GetSourceFiles> GetSourceFiles<TStep, TData>(this IStepBuilder<TData, TStep> builder)
        where TStep : IStepBody
        where TData : WorkflowData
        => builder
            .Then<TStep, GetSourceFiles, TData>()
            .Output(data => data.SourceRegistrations, step => step.SourceRegistrations);
}