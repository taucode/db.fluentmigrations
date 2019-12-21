using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace TauCode.Db.FluentMigrations
{
    public class FluentDbMigrator : IDbMigrator
    {
        #region Fields

        private readonly Dictionary<Type, object> _singletons;

        #endregion

        #region Constructor

        public FluentDbMigrator(string dbProviderName, string connectionString, Assembly migrationsAssembly)
        {
            this.DbProviderName = dbProviderName;
            this.ConnectionString = connectionString;
            this.MigrationsAssembly = migrationsAssembly;
            _singletons = new Dictionary<Type, object>();
        }

        #endregion

        #region Public

        public string DbProviderName { get; }

        public string ConnectionString { get; }

        public Assembly MigrationsAssembly { get; }

        public void AddSingleton(Type serviceType, object serviceImplementation)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (serviceImplementation == null)
            {
                throw new ArgumentNullException(nameof(serviceImplementation));
            }

            _singletons.Add(serviceType, serviceImplementation);
        }

        public IReadOnlyDictionary<Type, object> Singletons => _singletons;

        #endregion

        #region IUtility Members

        public IDbConnection Connection => null;
        public IUtilityFactory Factory => null;

        #endregion

        #region IDbMigrator Members

        public void Migrate()
        {
            if (string.IsNullOrWhiteSpace(this.ConnectionString))
            {
                throw new InvalidOperationException("Connection string must not be empty.");
            }

            if (this.MigrationsAssembly == null)
            {
                throw new InvalidOperationException("'MigrationsAssembly' must not be null.");
            }

            var serviceCollection = new ServiceCollection()
                // Add common FluentMigrator services
                .AddFluentMigratorCore();

            foreach (var pair in _singletons)
            {
                var type = pair.Key;
                var impl = pair.Value;

                serviceCollection.AddSingleton(type, impl);
            }

            var serviceProvider = serviceCollection
                .ConfigureRunner(rb =>
                {
                    switch (this.DbProviderName)
                    {
                        case DbProviderNames.SQLite:
                            rb.AddSQLite();
                            break;

                        case DbProviderNames.SqlServer:
                            rb.AddSqlServer();
                            break;

                        default:
                            throw new NotSupportedException($"'{DbProviderName}' not supported.");
                    }

                    rb
                        // Set the connection string
                        .WithGlobalConnectionString(this.ConnectionString)
                        // Define the assembly containing the migrations
                        .ScanIn(this.MigrationsAssembly).For.Migrations();
                })
                // Enable logging to console in the FluentMigrator way
                .AddLogging(lb => lb.AddFluentMigratorConsole())
                // Build the service provider
                .BuildServiceProvider(false);

            // Put the database update into a scope to ensure
            // that all resources will be disposed.
            using (serviceProvider.CreateScope())
            {
                // Instantiate the runner
                var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

                // Execute the migrations
                runner.MigrateUp();
            }
        }

        #endregion
    }
}
