using Sanchez.Models.CommandLine;
using Sanchez.Test.Common;
using Sanchez.Validators;

namespace Sanchez.Processing.Test.Validators;

#nullable enable

public abstract class AbstractValidatorTests<T, TOptions> : AbstractTests
    where T : CommandLineOptionsValidator<TOptions>, new() 
    where TOptions : CommandLineOptions
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