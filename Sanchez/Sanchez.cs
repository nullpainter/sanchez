using System.Runtime.CompilerServices;
using CommandLine;
using Sanchez.Builders;
using Sanchez.Models;
using Sanchez.Services;
using SimpleInjector;

[assembly: InternalsVisibleTo("Sanchez.Test")]
namespace Sanchez
{
    internal static class Sanchez
    {
        /// <summary>
        ///     Main entry point to application, parsing command-line arguments and creating composite image.
        /// </summary>
        internal static void Main(params string[] args)
        {
            var container = new Container().AddAllService();
            container.Verify();
            
            Parser.Default.ParseArguments<CommandLineOptions>(args).WithParsed(options =>
            {
                // Perform additional validation on input options
                var validator = container.GetInstance<IOptionValidator>();
                if (!validator.Validate(options)) return;
                
                // Composite images
                var compositor = container.GetInstance<ICompositor>();
                compositor.Create(options);
            });
        }
    }
}