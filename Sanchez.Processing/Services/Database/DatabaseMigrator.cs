using System.Reflection;
using DbUp;
using Microsoft.Extensions.Logging;

namespace Sanchez.Processing.Services.Database;

public interface IDatabaseMigrator
{
    /// <summary>
    ///     Executes SQL scripts to create or migrate database schema.
    /// </summary>
    void Migrate(string connectionString);
}
    
public class DatabaseMigrator(ILogger<DatabaseMigrator> logger) : IDatabaseMigrator
{
    public void Migrate(string connectionString)
    {
        var upgrader =
            DeployChanges.To
                .SqliteDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .LogToConsole()
                .Build();

        var result = upgrader.PerformUpgrade();
            
        if (!result.Successful)
        {
            logger.LogError("Unable to perform data migration: {Error}", result.Error);
            throw new InvalidOperationException("Fatal error initialising database");
        }
            
        logger.LogInformation("Database migrated");
    }
}