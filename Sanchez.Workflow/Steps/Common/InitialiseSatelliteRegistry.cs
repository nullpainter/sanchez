using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sanchez.Processing.Services;
using Sanchez.Shared.Exceptions;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Common
{
    /// <summary>
    ///     Registers all known satellites.
    /// </summary>
    internal class InitialiseSatelliteRegistry : StepBodyAsync
    {
        private readonly ISatelliteRegistry _satelliteRegistry;
        private readonly ILogger<InitialiseSatelliteRegistry> _logger;

        public InitialiseSatelliteRegistry(
            ISatelliteRegistry satelliteRegistry, 
            ILogger<InitialiseSatelliteRegistry> logger)
        {
            _satelliteRegistry = satelliteRegistry;
            _logger = logger;
        }

        public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            try
            {
                await _satelliteRegistry.InitialiseAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to parse satellite definition file");
                throw new ValidationException("Unable to parse satellite definition file; check logs for details.");
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
}