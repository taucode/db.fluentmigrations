using TauCode.Db.Extensions;

namespace TauCode.Db.Lab.Extensions
{
    public static class DbSchemaExplorerExtensionsLab
    {

        public static void DropAllTablesLab(this IDbSchemaExplorer schemaExplorer, string schemaName)
        {
            var tableNames = schemaExplorer.GetTableNames(schemaName, false);

            foreach (var tableName in tableNames)
            {
                schemaExplorer.DropTable(schemaName, tableName);
            }
        }
    }
}
