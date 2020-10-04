using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sanchez.Processing.Extensions
{
    public static class TaskExtensions
    {
        /// <summary>
        ///     Perform an async <see cref="Task" /> on a collection, limiting concurrency
        /// </summary>
        public static Task ForEachAsync<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, Task> taskSelector, int maxParallelisation = 1)
        {
            var oneAtATime = new SemaphoreSlim(maxParallelisation, maxParallelisation);
            return Task.WhenAll(
                from item in source
                select ProcessAsync(item, taskSelector, oneAtATime));
        }

        /// <summary>
        ///     Process a <see cref="Task" /> using a semaphore, allowing for controlled concurrent task execution
        /// </summary>
        /// <typeparam name="TSource">item type</typeparam>
        /// <param name="item">item to use as input to ask</param>
        /// <param name="taskSelector">task to execute</param>
        /// <param name="oneAtATime">semaphore to control concurrency</param>
        /// <returns></returns>
        private static async Task ProcessAsync<TSource>(
            TSource item,
            Func<TSource, Task> taskSelector,
            SemaphoreSlim oneAtATime)
        {
            // Execute task
            await taskSelector(item);

            // Wait for the semaphore to be available. This will block if the semaphore has reached
            // its maximum level of parallel tasks.
            await oneAtATime.WaitAsync();

            // The task has completed, allow the next task to be performed against the semaphore.
            oneAtATime.Release();
        }
    }
}