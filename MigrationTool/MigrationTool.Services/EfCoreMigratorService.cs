using MigrationTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationTool.Services
{
    public class EfCoreMigratorService
    {
        public static void GenerateEfCoreFiles(string connectionString, string schema, string contextName, string fileNamespace, string outputDirectory)
        {
            var databaseStructure = DatabaseService.GetDatabaseStructure(connectionString, schema);
            ContextGenerator.GenerateContextForDatabase(databaseStructure, fileNamespace, contextName, outputDirectory);
            var tableLists = GetElementsGroupedByTable(databaseStructure.Tables);
            foreach (var table in tableLists)
            {
                var tableName = table.First().TableName;
                DbSetGenerator.GenerateModelForTable(table, fileNamespace, tableName, outputDirectory);
            }
        }
        public static List<List<DatabaseElement>> GetElementsGroupedByTable(List<DatabaseElement> elements)
        {
            return elements
                .GroupBy(e => e.TableName)
                .Select(group => group.ToList())
                .ToList();
        }
    }
}
