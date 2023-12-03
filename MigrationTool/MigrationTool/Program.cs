using Microsoft.Data.SqlClient;
using MigrationTool.Services;
using System.Data;
using System.Text;

namespace MigrationTool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "Data Source={databaseServer};Initial Catalog={databaseName};Integrated Security=true;";
            string schema = "";
            string namespaceName = "";
            string outputDirectory = "";
            string fileNamespace = "";
            string contextName = "";

            EfCoreMigratorService.GenerateEfCoreFiles(connectionString, schema, contextName, fileNamespace, outputDirectory)
       }
    }
}