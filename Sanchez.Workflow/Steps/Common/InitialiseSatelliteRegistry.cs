using FluentValidation;
using Microsoft.Extensions.Logging;
using Sanchez.Processing.Services;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Common;

/// <summary>
///     Registers all known satellites.
/// </summary>
internal class InitialiseSatelliteRegistry(
    ISatelliteRegistry satelliteRegistry,
    ILogger<InitialiseSatelliteRegistry> logger) : StepBodyAsync
{
    public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        try
        {
            await satelliteRegistry.InitialiseAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to parse satellite or image path definition file");
            throw new ValidationException("Unable to parse satellite or image path definition file; check logs for details.");
        }

        return ExecutionResult.Next();
    }
}

internal static class InitialiseSatelliteRegistryExtensions
{
    internal static IStepBuilder<TData, InitialiseSatelliteRegistry> InitialiseSatelliteRegistry<TStep, TData>(this IStepBuilder<TData, TStep> builder)
        where TStep : IStepBody
        where TData : WorkflowData
        => builder
            .Then<TStep, InitialiseSatelliteRegistry, TData>("Initialise satellite registry");
}