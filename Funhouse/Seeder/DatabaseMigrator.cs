using System;
using System.Reflection;
using DbUp;
using Serilog;

namespace Funhouse.Seeder
{
    public interface IDatabaseMigrator
    {
        void Migrate(string connectionString);
    }
    
    public class DatabaseMigrator : IDatabaseMigrator
    {
        public void Migrate(string connectionString)
        {
            var upgrader =
                DeployChanges.To
                    .SQLiteDatabase(connectionString)
                    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                    .LogToConsole()
                    .Build();

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                Log.Error("Unable to perform data migration: {error}", result.Error);
                throw new InvalidOperationException("Fatal error initialising database");
            }
            
            Log.Information("Database migrated");
        }
    }
}