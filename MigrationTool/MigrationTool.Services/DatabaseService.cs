using Microsoft.Data.SqlClient;
using MigrationTool.Models;

namespace MigrationTool.Services;

public class DatabaseService
    {
        public static DatabaseStructure GetDatabaseStructure(string connectionString, string schema)
        {
            DatabaseStructure databaseStructure = new DatabaseStructure();
            List<DatabaseElement> databaseElements = new List<DatabaseElement>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query =  "SELECT " +
                                "    col.TABLE_NAME, " +
                                "    col.COLUMN_NAME, " +
                                "    col.DATA_TYPE, " +
                                "    col.CHARACTER_MAXIMUM_LENGTH, " +
                                "    col.IS_NULLABLE," +
                                "    kcu.CONSTRAINT_NAME AS FK_CONSTRAINT_NAME," +
                                "    kcu.TABLE_NAME," +
                                "    kcu.COLUMN_NAME" +
                                "FROM " +
                                "    INFORMATION_SCHEMA.COLUMNS col" +
                                "LEFT JOIN " +
                                "    INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu ON col.TABLE_SCHEMA = kcu.TABLE_SCHEMA" +
                                "    AND col.TABLE_NAME = kcu.TABLE_NAME " +
                                "    AND col.COLUMN_NAME = kcu.COLUMN_NAME" +
                                "    AND kcu.ORDINAL_POSITION IS NOT NULL" +
                                "LEFT JOIN " +
                                "    INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc ON kcu.CONSTRAINT_NAME = rc.CONSTRAINT_NAME" +
                                "    AND kcu.TABLE_SCHEMA = rc.CONSTRAINT_SCHEMA" +
                                "WHERE " +
                               $"    col.TABLE_SCHEMA = '{schema}'";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    DatabaseElement element = new DatabaseElement();
                    element.TableName = reader["TABLE_NAME"].ToString();
                    element.ColumnName = reader["COLUMN_NAME"].ToString();
                    element.DataType = reader["DATA_TYPE"].ToString();
                    element.MaxLength = reader["CHARACTER_MAXIMUM_LENGTH"] != DBNull.Value ? Convert.ToInt32(reader["CHARACTER_MAXIMUM_LENGTH"]) : 0;
                    element.IsNullable = bool.Parse(reader["IS_NULLABLE"].ToString());
                    element.ForeignKeyConstraintName = reader["FK_CONSTRAINT_NAME"].ToString();
                    element.ForeignKeyTableName = reader["TABLE_NAME"].ToString();
                    element.ForeignKeyColumnName = reader["COLUMN_NAME"].ToString();

                    databaseElements.Add(element);
                }

                reader.Close();
            }

            databaseStructure.Tables = databaseElements;
            return databaseStructure;
        }
    }