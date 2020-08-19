using System.IO;
using System.Threading.Tasks;
using Funhouse.Services;
using NUnit.Framework;

namespace Funhouse.Test
{
    public abstract class AbstractTests
    {
        /// <summary>
        ///     Small number for floating-point comparison tests.
        /// </summary>
        protected const float Precision = 0.000001f;

        private static string RegistryPath => Path.Combine(TestContext.CurrentContext.TestDirectory, Constants.DefinitionsPath);

        internal readonly SatelliteRegistry SatelliteRegistry = new SatelliteRegistry(RegistryPath);

        [SetUp]
        public async Task SetupAsync() => await SatelliteRegistry.InitialiseAsync();
    }
}