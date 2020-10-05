using System;
using System.Reflection;
using DbUp;
using Microsoft.Extensions.Logging;

namespace Sanchez.Processing.Services.Database
{
    public interface IDatabaseMigrator
    {
        /// <summary>
        ///     Executes SQL scripts to create or migrate database schema.
        /// </summary>
        void Migrate(string connectionString);
    }
    
    public class DatabaseMigrator : IDatabaseMigrator
    {
        private readonly ILogger<DatabaseMigrator> _logger;

        public DatabaseMigrator(ILogger<DatabaseMigrator> logger) => _logger = logger;

        public void Migrate(string connectionString)
        {
            var upgrader =
                DeployChanges.To
                    .SQLiteDatabase(connectionString)
                    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                    .LogToAutodetectedLog()
                    .Build();

            var result = upgrader.PerformUpgrade();
            
            if (!result.Successful)
            {
                _logger.LogError("Unable to perform data migration: {error}", result.Error);
                throw new InvalidOperationException("Fatal error initialising database");
            }
            
            _logger.LogInformation("Database migrated");
        }
    }
}