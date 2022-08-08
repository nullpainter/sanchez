using FluentValidation;
using Microsoft.Extensions.Logging;
using Sanchez.Processing.Services;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Common;

public class InitialiseGradient : StepBodyAsync
{
    private readonly ILogger<InitialiseGradient> _logger;
    private readonly IGradientService _gradientService;

    public InitialiseGradient(ILogger<InitialiseGradient> logger, IGradientService gradientService)
    {
        _logger = logger;
        _gradientService = gradientService;
    }
    public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        try
        {
            await _gradientService.InitialiseAsync();
        }
        catch (ValidationException e)
        {
            _logger.LogError(e, "Unable to parse gradient file");
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unable to parse gradient file");
            throw new ValidationException("Unable to parse gradient file; check logs for details.");
        }

        return ExecutionResult.Next();
    }
}
    
    
internal static class InitialiseGradientExtensions
{
    internal static IStepBuilder<TData, InitialiseGradient> InitialiseGradient<TStep, TData>(this IStepBuilder<TData, TStep> builder)
        where TStep : IStepBody
        where TData : WorkflowData
        => builder
            .Then<TStep, InitialiseGradient, TData>("Initialise gradient");
}