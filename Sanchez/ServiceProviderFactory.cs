using Microsoft.Extensions.DependencyInjection;
using Sanchez.Builders;
using Sanchez.Processing.Models;

namespace Sanchez
{
    public static class ServiceProviderFactory
    {
        public static ServiceProvider ConfigureServices(RenderOptions renderOptions)
        {
            var serviceProvider = new ServiceCollection()
                .AddWorkflow()
                .ConfigureLogging(renderOptions.Verbose)
                .AddApplicationServices(renderOptions)
                .BuildServiceProvider(true);
            
            return serviceProvider;
        }
    }
}