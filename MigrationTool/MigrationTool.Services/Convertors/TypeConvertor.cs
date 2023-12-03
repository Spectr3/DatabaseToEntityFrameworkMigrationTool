namespace MigrationTool.Services;

public class TypeConvertor
{
    public static string GetCSharpType(string dataType, bool isNullable)
    {
        switch (dataType)
        {
            case "tinyint":
                return isNullable ? "byte?" : "byte";

            case "smallint":
                return isNullable ? "short?" : "short";

            case "int":
                return isNullable ? "int?" : "int";

            case "bigint":
                return isNullable ? "long?" : "long";

            case "bit":
                return isNullable ? "bool?" : "bool";

            case "varchar":
            case "nvarchar":
            case "text":
            case "ntext":
            case "char":
            case "nchar":
                return "string";

            case "datetime":
            case "smalldatetime":
            case "datetime2":
            case "date":
            case "time":
                return isNullable ? "DateTime?" : "DateTime";

            case "timestamp":
            case "rowversion":
            case "binary":
            case "varbinary":
            case "image":
                return "byte[]";

            case "uniqueidentifier":
                return isNullable ? "Guid?" : "Guid";

            case "float":
                return isNullable ? "double?" : "double";

            case "real":
                return isNullable ? "float?" : "float";

            case "money":
            case "smallmoney":
            case "decimal":
            case "numeric":
                return isNullable ? "decimal?" : "decimal";

            case "xml":
                return "string";  // or System.Xml.Linq.XElement based on use case

            case "sql_variant":
                return "object";

            case "geometry":
            case "geography":
                return "Microsoft.SqlServer.Types.SqlGeometry"; // or SqlGeography based on data type

            case "hierarchyid":
                return "Microsoft.SqlServer.Types.SqlHierarchyId";

            default:
                throw new Exception($"Unknown data type '{dataType}'");
        }
    }
}