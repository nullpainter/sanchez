using System;
using System.Reflection;
using DbUp;
using Serilog;

namespace Sanchez.Seeder
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
                Log.Error("Unable to perform data migration: {error}", result.Error);
                throw new InvalidOperationException("Fatal error initialising database");
            }
            
            Log.Information("Database migrated");
        }
    }
}