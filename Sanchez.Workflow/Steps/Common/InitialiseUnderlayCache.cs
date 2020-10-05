using Sanchez.Processing.Services.Underlay;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models;
using Sanchez.Workflow.Models.Data;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Common
{
    internal class InitialiseUnderlayCache : StepBody
    {
        private readonly IUnderlayCacheRepository _cacheRepository;

        public InitialiseUnderlayCache(IUnderlayCacheRepository cacheRepository) => _cacheRepository = cacheRepository;

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            _cacheRepository.Initialise();
            return ExecutionResult.Next();
        }
    }

    internal static class InitialiseUnderlayCacheExtensions
    {
        internal static IStepBuilder<TData, InitialiseUnderlayCache> InitialiseUnderlayCache<TData>(this IWorkflowBuilder<TData> builder)
            where TData : WorkflowData
            => builder
                .StartWith<InitialiseUnderlayCache, TData>("Initialise underlay cache");
    }
}