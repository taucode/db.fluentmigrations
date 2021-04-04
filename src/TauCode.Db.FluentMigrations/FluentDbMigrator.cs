using FluentMigrator.Runner;
using FluentMigrator.Runner.VersionTableInfo;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace TauCode.Db.FluentMigrations
{
    public class FluentDbMigrator : IDbMigrator
    {
        #region Constants

        protected const string DefaultVersionTableName = "VersionInfo";
        protected const string DefaultVersionTableVersionColumnName = "Version";
        protected const string DefaultVersionTableDescriptionColumnName = "Description";
        protected const string DefaultVersionTableUniqueIndexName = "UC_Version";
        protected const string DefaultVersionTableAppliedOnColumnName = "AppliedOn";

        #endregion

        #region Nested

        protected class VersionTableMetaData : IVersionTableMetaData
        {
            public VersionTableMetaData(
                string schemaName,
                string tableName = DefaultVersionTableName,
                string columnName = DefaultVersionTableVersionColumnName,
                string descriptionColumnName = DefaultVersionTableDescriptionColumnName,
                string uniqueIndexName = DefaultVersionTableUniqueIndexName,
                string appliedOnColumnName = DefaultVersionTableAppliedOnColumnName)
            {
                this.SchemaName = schemaName;
                this.TableName = tableName;
                this.ColumnName = columnName;
                this.DescriptionColumnName = descriptionColumnName;
                this.UniqueIndexName = uniqueIndexName;
                this.AppliedOnColumnName = appliedOnColumnName;
            }

            public object ApplicationContext { get; set; }
            public bool OwnsSchema => false;
            public string SchemaName { get; }
            public string TableName { get; }
            public string ColumnName { get; }
            public string DescriptionColumnName { get; }
            public string UniqueIndexName { get; }
            public string AppliedOnColumnName { get; }
        }

        #endregion

        #region Fields

        private readonly Dictionary<Type, object> _singletons;

        #endregion

        #region Constructor

        public FluentDbMigrator(string dbProviderName, string connectionString, string schemaName, Assembly migrationsAssembly)
        {
            this.DbProviderName = dbProviderName;
            this.ConnectionString = connectionString;
            this.SchemaName = schemaName;
            this.MigrationsAssembly = migrationsAssembly;
            _singletons = new Dictionary<Type, object>();

            this.AddSingleton(typeof(ISchemaNameContainer), new SchemaNameContainer(this.SchemaName));
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

        public IDbUtilityFactory Factory => null;

        #endregion

        #region IDbMigrator Members

        public virtual void Migrate()
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

                        case DbProviderNames.SQLServer:
                            rb.AddSqlServer();
                            break;

                        case DbProviderNames.PostgreSQL:
                            rb.AddPostgres();
                            break;

                        case DbProviderNames.MySQL:
                            rb.AddMySql5();
                            break;

                        default:
                            throw new NotSupportedException($"'{DbProviderName}' not supported.");
                    }

                    rb
                        // Set the connection string
                        .WithGlobalConnectionString(this.ConnectionString)
                        // Define the assembly containing the migrations
                        .ScanIn(this.MigrationsAssembly).For.Migrations();

                    if (this.SchemaName != null)
                    {
                        var versionTableMetaData = this.CreateVersionTableMetaData();

                        rb.WithVersionTable(versionTableMetaData);
                    }
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

        protected virtual IVersionTableMetaData CreateVersionTableMetaData()
        {
            IVersionTableMetaData versionTableMetaData = new VersionTableMetaData(this.SchemaName);
            return versionTableMetaData;
        }

        public string SchemaName { get; }

        #endregion
    }
}
