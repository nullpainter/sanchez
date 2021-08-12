using System;
using ShellProgressBar;

namespace Sanchez.Processing.Services
{
    /// <summary>
    ///     Progress bar which writes no output. This is required to be used if no console is available.
    /// </summary>
    public sealed class NullProgressBar : ProgressBarBase, IProgressBar
    {
        public NullProgressBar() : base(0, null, null)
        {
        }

        protected override void DisplayProgress()
        {
        }

        public override void WriteLine(string message)
        {
        }

        public override void WriteErrorLine(string message)
        {
        }

        public IProgress<T> AsProgress<T>(Func<T, string>? message = null, Func<T, double?>? percentage = null) => new Progress<T>();
        
        public void Dispose()
        {
            // Nothing to dispose
        }
    }
}