using Funhouse.Models;
using Funhouse.Services;
using ShellProgressBar;

namespace Funhouse.Helpers
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

            return new ProgressBar(0, initialMessage, progressBarOptions);
        }
    }
}