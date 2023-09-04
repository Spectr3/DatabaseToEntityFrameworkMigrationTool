using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace MigrationTool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "Data Source={databaseServer};Initial Catalog={databaseName};Integrated Security=true;";
            string namespaceName = "";
            string outputDirectory = "";
            string contextNamespace = "";
            string contextName = "";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Retrieve the list of tables and columns in the database
                string tablesQuery = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'";
                string columnsQuery = "SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS";
                SqlDataAdapter adapter = new SqlDataAdapter($"{tablesQuery};{columnsQuery}", connection);
                DataSet dataset = new DataSet();
                adapter.Fill(dataset);

                // Group the columns by table name
                Dictionary<string, List<DataRow>> columnsByTable = dataset.Tables[1].AsEnumerable().GroupBy(r => r.Field<string>("TABLE_NAME")).ToDictionary(g => g.Key, g => g.ToList());

                StringBuilder dbContextCode = new StringBuilder();

                dbContextCode.AppendLine($"using Microsoft.EntityFrameworkCore;");
                dbContextCode.AppendLine($"using {namespaceName};"); // Assuming your entities are in this namespace
                dbContextCode.AppendLine();
                dbContextCode.AppendLine($"namespace {contextNamespace}");
                dbContextCode.AppendLine("{");
                dbContextCode.AppendLine($"    public class {contextName} : DbContext");
                dbContextCode.AppendLine("    {");
                dbContextCode.AppendLine($"        public {contextName}(DbContextOptions<{contextName}> options) : base(options) {{ }}");
                dbContextCode.AppendLine();

                // Generate the C# code for each table
                foreach (DataRow tableRow in dataset.Tables[0].Rows)
                {
                    string tableName = tableRow.Field<string>("TABLE_NAME");
                    List<DataRow> columnRows;
                    if (!columnsByTable.TryGetValue(tableName, out columnRows))
                    {
                        continue;
                    }

                    // Generate the C# code for the model class
                    string modelCode = "using System;\n\nnamespace " + namespaceName + "\n{\n    public class " + tableName + "\n    {\n";
                    foreach (DataRow columnRow in columnRows)
                    {
                        string columnName = columnRow.Field<string>("COLUMN_NAME");
                        string dataType = columnRow.Field<string>("DATA_TYPE");
                        int? maxLength = columnRow.IsNull("CHARACTER_MAXIMUM_LENGTH") ? null : (int?)columnRow.Field<int>("CHARACTER_MAXIMUM_LENGTH");
                        bool isNullable = columnRow.Field<string>("IS_NULLABLE") == "YES";

                        string csharpType = GetCSharpType(dataType, maxLength, isNullable);
                        modelCode += $"        public {csharpType} {columnName} {{ get; set; }}\n";
                    }
                    modelCode += "    }\n}\n";

                    // Write the generated C# code to a file
                    string filename = outputDirectory + tableName + ".cs";
                    File.WriteAllText(filename, modelCode);

                    dbContextCode.AppendLine($"        public DbSet<{tableName}> {tableName} {{ get; set; }}");
                }
                dbContextCode.AppendLine("    }");
                dbContextCode.AppendLine("}");

                // Write the DbContext to a file
                string dbContextFilename = Path.Combine(outputDirectory, $"{contextName}.cs");
                File.WriteAllText(dbContextFilename, dbContextCode.ToString());
            }

            static string GetCSharpType(string dataType, int? maxLength, bool isNullable)
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
}