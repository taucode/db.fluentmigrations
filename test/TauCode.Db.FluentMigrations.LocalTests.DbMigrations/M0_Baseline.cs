using FluentMigrator;

namespace TauCode.Db.FluentMigrations.LocalTests.DbMigrations;

[Migration(0)]
public class M0_Baseline : AutoReversingMigration
{
    public M0_Baseline(ISchemaNameContainer schemaNameContainer)
    {
        this.SchemaName = schemaNameContainer.SchemaName;
    }

    public string SchemaName { get; }

    public override void Up()
    {
        this.Create
            .Table("Person")
            .InSchema(this.SchemaName)
            .WithColumn("Uid")
            .AsGuid()
            .PrimaryKey()
            .WithColumn("Name")
            .AsString();
    }
}