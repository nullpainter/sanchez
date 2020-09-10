using System;

namespace Funhouse.Exceptions
{
    /// <summary>
    ///     Exception thrown when a pre-condition hasn't been met.
    /// </summary>
    public class ValidationException : Exception
    {
        public ValidationException()
        {
        }

        public ValidationException(string message) 
            : base(message)
        {
        }
    }
}