using Microsoft.Data.SqlClient;
using System.Data;

namespace MigrationTool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "Data Source={databaseServer};Initial Catalog={databaseName};Integrated Security=true;";
            string namespaceName = "";
            string outputDirectory = "";

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
                }
            }

            static string GetCSharpType(string dataType, int? maxLength, bool isNullable)
            {
                if (dataType == "int")
                {
                    return isNullable ? "int?" : "int";
                }
                else if (dataType == "varchar" || dataType == "nvarchar" || dataType == "text" || dataType == "ntext")
                {
                    return "string";
                }
                else if (dataType == "datetime" || dataType == "smalldatetime")
                {
                    return isNullable ? "DateTime?" : "DateTime";
                }
                else if (dataType == "varbinary")
                {
                    return "byte[]";
                }
                // Add other data types here as needed
                else
                {
                    throw new Exception($"Unknown data type '{dataType}'");
                }
            }
    }
}