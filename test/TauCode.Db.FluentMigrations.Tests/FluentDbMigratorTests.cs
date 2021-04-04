using Npgsql;
using NUnit.Framework;
using TauCode.Db.FluentMigrations.Tests.DbMigrations;
using TauCode.Db.Lab.Extensions;
using TauCode.Db.Npgsql;

namespace TauCode.Db.FluentMigrations.Tests
{
    [TestFixture]
    public class FluentDbMigratorTests
    {
        internal const string ConnectionString = @"User ID=postgres;Password=1234;Host=localhost;Port=5432;Database=my_tests";

        [Test]
        public void Migrate_ValidInput_Migrates()
        {
            // Arrange
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            var schemaExplorer = new NpgsqlSchemaExplorer(connection);
            schemaExplorer.DropAllTablesLab("zeta");

            // Act
            var migrator = new FluentDbMigrator(DbProviderNames.PostgreSQL, ConnectionString, "zeta", typeof(M0_Baseline).Assembly);
            migrator.Migrate();

            // Assert
            var tableNames = schemaExplorer.GetTableNames("zeta");
            Assert.That(tableNames, Does.Contain("Person"));
        }
    }
}
