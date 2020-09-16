using System;
using ShellProgressBar;

namespace Funhouse.Services
{
    /// <summary>
    ///     Progress bar which writes no output. This is required to be used if no console is available.
    /// </summary>
    public class NullProgressBar : ProgressBarBase, IProgressBar
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

        public IProgress<T> AsProgress<T>(Func<T, string>? message = null, Func<T, double?>? percentage = null) => new Progress<T>();
        
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Nothing to dispose
        }
    }
}