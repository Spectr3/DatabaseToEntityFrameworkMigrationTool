using System.Text;
using MigrationTool.Models;

namespace MigrationTool.Services;

public class DbSetGenerator
{
    public static void GenerateModelForTable(List<DatabaseElement> elements, string fileNamespace, string tableName, string outputDirectory)
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

    private static string GenerateModelText(List<DatabaseElement> elements, string fileNamespace, string tableName)
    {
        StringBuilder fileText = new StringBuilder();

        fileText.AppendLine("using System.ComponentModel.DataAnnotations;\n\n");
        fileText.AppendLine("namespace " + fileNamespace + ".Models\n{\n");
        fileText.AppendLine("\tpublic class " + tableName + "\n{\n");

        foreach (var row in elements)
        {
            GetAnnotationsForProperty(fileText, row);
            fileText.AppendLine(GetColumnAnnotation(row));
            if (row.MaxLength.HasValue)
                fileText.AppendLine(GetMaxLengthAnnotation(row));
        }

        fileText.AppendLine("}\n}\n");

        return fileText.ToString();
    }

    private static void GetAnnotationsForProperty(StringBuilder fileText, DatabaseElement row)
    {
        if (row.MaxLength.HasValue)
        {
            fileText.AppendLine($"\t\t[MaxLength({row.MaxLength})]");
        }
        foreach (var constraint in row.Constraints)
        {
            fileText.AppendLine($"\t\t{Constraint.GetAnnotationForConstraint(constraint.Type)}");
        }
    }

    private static string GetMaxLengthAnnotation(DatabaseElement row)
    {
        return "\t\t[MaxLength(" + row.MaxLength + ")]";
    }

    private static string GetColumnAnnotation(DatabaseElement row)
    {
        return "\t\t[Column(\"" + row.ColumnName + "\", TypeName = \"" + row.DataType + "\")]";
    }   
}