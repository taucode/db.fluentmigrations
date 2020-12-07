using NUnit.Framework;
using TauCode.Db.FluentMigrations.Tests.DbMigrations;

namespace TauCode.Db.FluentMigrations.Tests
{
    [TestFixture]
    public class FooFixture
    {
        internal const string ConnectionString = @"User ID=postgres;Password=1234;Host=localhost;Port=5432;Database=my_tests";

        [Test]
        public void Todo()
        {
            var migrator = new FluentDbMigrator(DbProviderNames.PostgreSQL, ConnectionString, "zeta", typeof(M0_Baseline).Assembly);
            migrator.Migrate();
        }
    }
}
