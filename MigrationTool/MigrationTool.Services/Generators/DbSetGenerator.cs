using System.Text;
using MigrationTool.Models;

namespace MigrationTool.Services;

public class DbSetGenerator
{
    public void GenerateModelForTable(List<DatabaseElement> elements, string fileNamespace, string tableName, string outputDirectory)
    {
        var fileText = GenerateModelText(elements, fileNamespace, tableName);

        var modelsDirectory = Path.Combine(outputDirectory, "Models");
        if (!Directory.Exists(modelsDirectory))
        {
            Directory.CreateDirectory(modelsDirectory);
        }

        var fileName = Path.Combine(modelsDirectory, $"{tableName}.cs");
        File.WriteAllText(fileName, fileText);
    }

    public string GenerateModelText(List<DatabaseElement> elements, string fileNamespace, string tableName)
    {
        StringBuilder fileText = new StringBuilder();

        fileText.AppendLine("using System.ComponentModel.DataAnnotations;\n\n");
        fileText.AppendLine("namespace " + fileNamespace + ".Models\n{\n");
        fileText.AppendLine("public class " + tableName + "\n{\n");

        foreach (var row in elements)
        {
            fileText.AppendLine(GetColumnAnnotation(row));
            if (row.MaxLength.HasValue)
                fileText.AppendLine(GetMaxLengthAnnotation(row));
            fileText.AppendLine(GetPropertyForColumn(row));
        }

        fileText.AppendLine("}\n}\n");

        return fileText.ToString();
    }

    private string GetMaxLengthAnnotation(DatabaseElement row)
    {
        return "[MaxLength(" + row.MaxLength + ")]";
    }

    private string GetColumnAnnotation(DatabaseElement row)
    {
        return "[Column(\"" + row.ColumnName + "\", TypeName = \"" + row.DataType + "\")]";
    }

    private string GetPropertyForColumn(DatabaseElement row)
    {
        var cSharpDataType = TypeConvertor.GetCSharpType(row.DataType, row.IsNullable);
        return $"public {cSharpDataType} {row.ColumnName} {{ get; set; }}";
    }
}