using System;
using FluentValidation.Results;

namespace Funhouse.Exceptions
{
    /// <summary>
    ///     Exception thrown when a pre-condition hasn't been met.
    /// </summary>
    public class ValidationException : Exception
    {
        public ValidationResult? Result { get; }

        public ValidationException()
        {
            
        }
        public ValidationException(ValidationResult result) => Result = result;

        public ValidationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public ValidationException(string message) 
            : base(message)
        {
        }
    }
}