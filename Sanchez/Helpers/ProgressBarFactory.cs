using System.IO;
using Sanchez.Models;
using Sanchez.Services;
using ShellProgressBar;

namespace Sanchez.Helpers
{
    public static class ProgressBarFactory
    {
        public static IProgressBar NewProgressBar(RenderOptions options, string? initialMessage = null)
        {
            if (options.Quiet || options.Verbose) return new NullProgressBar();
            
            var progressBarOptions = new ProgressBarOptions
            {
                ProgressCharacter = '─',
                ProgressBarOnBottom = true
            };

            try
            {
                return new ProgressBar(int.MaxValue, initialMessage, progressBarOptions);
            }
            catch (IOException)
            {
                // Unable to get console buffer, so return a no-op progress bar
                return new NullProgressBar();
            }
        }
    }
}