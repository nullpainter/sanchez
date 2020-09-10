using System.Linq;
using FluentAssertions;
using Funhouse.Models.CommandLine;
using Funhouse.Validators;

namespace Funhouse.Test.Validators
{
    public abstract class AbstractValidatorTests<T, TOptions> where T : OptionsValidator<TOptions>, new() where TOptions : BaseOptions
    {
        protected static void VerifyFailure(TOptions options, string propertyName, string? message = null)
        {
            var results = new T().Validate(options);
            results.Errors.Select(e => e.PropertyName).Should().Contain(propertyName, "validation failures should be present for property");
            
            if (message != null)
            {
                results.Errors
                    .Where(e => e.PropertyName == propertyName)
                    .Select(e => e.ErrorMessage)
                    .Should().Contain(message);
            }
        }

        protected static void VerifyNoFailure(TOptions options, string propertyName)
        {
            var results = new T().Validate(options);
            results.Errors.Select(e => e.PropertyName).Should().NotContain(propertyName);
        }

        protected static void VerifyNoFailure(TOptions options)
        {
            var results = new T().Validate(options);
            results.Errors.Should().BeEmpty();
        }
    }
}