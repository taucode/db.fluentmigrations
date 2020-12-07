namespace TauCode.Db.FluentMigrations
{
    public class SchemaNameContainer : ISchemaNameContainer
    {
        public SchemaNameContainer(string schemaName)
        {
            this.SchemaName = schemaName;
        }

        public string SchemaName { get; }
    }
}
