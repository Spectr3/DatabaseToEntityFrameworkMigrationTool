using MigrationTool.Models;
using System.Text;

namespace MigrationTool.Services;

public class ContextGenerator
{
    public static void GenerateContextForDatabase(DatabaseStructure structure, string fileNamespace, string contextName, string outputDirectory)
    {
        var fileText = GetTextForContextFile(structure, fileNamespace, contextName);

        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        var fileName = Path.Combine(outputDirectory, $"{contextName}.cs");
        File.WriteAllText(fileName, fileText);
    }

    private static string GetTextForContextFile(DatabaseStructure structure, string fileNamespace, string contextName)
    {
        StringBuilder fileText = new StringBuilder();
        
        fileText.AppendLine($"using Microsoft.EntityFrameworkCore;");
        fileText.AppendLine($"using {fileNamespace}.Models;");
        fileText.AppendLine($"namespace {fileNamespace}");
        fileText.AppendLine("{");
        fileText.AppendLine($"\tpublic class {contextName}");
        fileText.AppendLine("\t{");
        fileText.AppendLine($"\t\tpublic {contextName} : DbContext");
        fileText.AppendLine($"\t\t{{");
        fileText.AppendLine($"\t\t\tpublic {contextName}(DbContextOptions<{contextName}> options) : base(options)");
        fileText.AppendLine("\t\t\t{");
        AddDbSetsToFileText(fileText, structure);
        fileText.AppendLine("\t\t\t}");
        fileText.AppendLine("\t\t}");
        fileText.AppendLine("\t}");
        fileText.AppendLine("}");

        return fileText.ToString();
    }

    private static void AddDbSetsToFileText(StringBuilder fileText, DatabaseStructure structure)
    {
        var tablesToAdd = GetUniqueByTableName(structure.Tables);
        foreach (var table in tablesToAdd)
        {
            fileText.AppendLine($"\t\t\t\tpublic DbSet<{table.TableName}> {table.TableName} {{ get; set; }}");
        }
    }

    private static List<DatabaseElement> GetUniqueByTableName(List<DatabaseElement> tables)
    {
        return tables
        .GroupBy(e => e.TableName)
        .Select(group => group.First())
        .ToList();
    }
}