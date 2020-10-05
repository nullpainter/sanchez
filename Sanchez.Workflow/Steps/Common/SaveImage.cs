using System;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using JetBrains.Annotations;
using Sanchez.Processing.Extensions.Images;
using Sanchez.Processing.Models.Projections;
using Sanchez.Workflow.Extensions;
using Sanchez.Workflow.Models.Data;
using Sanchez.Workflow.Models.Steps;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Sanchez.Workflow.Steps.Common
{
    internal sealed class SaveImage : StepBodyAsync, IRegistrationStepBody, IDisposable
    {
        public Registration? Registration { get; set; }
        public int RenderedCount { get; [UsedImplicitly] set; }
        public Image<Rgba32>? TargetImage { get; set; }

        /// <summary>
        ///     Dispose the registration after saving, as it's no longer used.
        /// </summary>
        public void Dispose() => Registration?.Dispose();

        public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            Guard.Against.Null(TargetImage, nameof(TargetImage));
            Guard.Against.Null(Registration?.OutputPath, nameof(Registration.OutputPath));
 
            // Save image
            using (TargetImage)
            using (Registration)
            {
                await TargetImage!.SaveWithExifAsync(Registration.OutputPath);
            }
            
            RenderedCount++;
            
            return ExecutionResult.Next();
        }
    }

    internal static class SaveImageExtensions
    {
        internal static IStepBuilder<TData, SaveImage> SaveImage<TStep, TData>(this IStepBuilder<TData, TStep> builder)
            where TStep : IStepBody
            where TData : WorkflowData
            => builder
                .Then<TStep, SaveImage, TData>("Save image")
                .WithRegistration()
                .Input(step => step.TargetImage, data => data.TargetImage)
                .Input(step => step.RenderedCount, data => data.RenderedCount)
                .Output(data => data.RenderedCount, step => step.RenderedCount);
    }
}