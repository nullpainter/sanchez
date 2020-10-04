using Ardalis.GuardClauses;
using Sanchez.Processing.Extensions.Images;
using Sanchez.Processing.Models;
using Sanchez.Processing.Models.Projections;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Common
{
    internal sealed class NormaliseImage : StepBody, IRegistrationStepBody
    {
        private readonly RenderOptions _options;

        public NormaliseImage(RenderOptions options) => _options = options;

        public Registration? Registration { get; set; }

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            Guard.Against.Null(Registration, nameof(Registration));

            Registration.Normalise(_options);
            return ExecutionResult.Next();
        }
    }

    internal static class NormaliseImageExtensions
    {
        internal static IStepBuilder<TData, NormaliseImage> NormaliseImage<TData, TStep>(this IStepBuilder<TData, TStep> builder)
            where TStep : IStepBody
            where TData : WorkflowData
        {
            return builder
                .Then<TStep, NormaliseImage, TData>("Normalise image")
                .WithRegistration();
        }
        
        internal static IStepBuilder<TData, NormaliseImage> NormaliseImage<TData>(this IWorkflowBuilder<TData> builder)
            where TData : WorkflowData
        {
            return builder
                .StartWith<NormaliseImage, TData>("Normalise image")
                .WithRegistration();
        }
    }
}