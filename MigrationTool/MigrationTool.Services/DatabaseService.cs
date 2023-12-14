using Microsoft.Data.SqlClient;
using MigrationTool.Models;

namespace MigrationTool.Services;

public class DatabaseService
    {
        public static DatabaseStructure GetDatabaseStructure(string connectionString, string schema)
        {
            var structure = new DatabaseStructure
            {
                Tables = new List<DatabaseElement>()
            };
            
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @schema", connection);
                command.Parameters.AddWithValue("@schema", schema);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var element = new DatabaseElement
                        {
                            TableName = reader["TABLE_NAME"].ToString(),
                            ColumnName = reader["COLUMN_NAME"].ToString(),
                            DataType = reader["DATA_TYPE"].ToString(),
                            MaxLength = reader["CHARACTER_MAXIMUM_LENGTH"] != DBNull.Value ? Convert.ToInt32(reader["CHARACTER_MAXIMUM_LENGTH"]) : 0,
                            IsNullable = GetBoolFromString(reader["IS_NULLABLE"].ToString())
                        };
                        structure.Tables.Add(element);
                    }
                    reader.Close();
                }

                foreach (var table in structure.Tables)
                {
                    command = new SqlCommand("SELECT CONSTRAINT_NAME, CONSTRAINT_TYPE FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE TABLE_SCHEMA = @schema AND TABLE_NAME = @table", connection);
                    command.Parameters.AddWithValue("@schema", schema);
                    command.Parameters.AddWithValue("@table", table.TableName);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var constraint = new Constraint
                            {
                                Name = reader["CONSTRAINT_NAME"].ToString(),
                                Type = Constraint.GetConstraintType(reader["CONSTRAINT_TYPE"].ToString())
                            };
                            table.Constraints.Add(constraint);
                        }
                        reader.Close();
                    }
                }
            }
            return structure;
        }

        private static ConstraintType? GetConstraintType(string input)
        {
            switch (input.ToUpper().Trim())
            {
                case ("FOREIGN KEY"):
                    return ConstraintType.ForeignKey;
                case ("PRIMARY KEY"):
                    return ConstraintType.PrimaryKey;
                case ("INDEX"):
                    return ConstraintType.Index;
                case ("UNIQUE"):
                    return ConstraintType.Unique;
                case ("CHECK"):
                    return ConstraintType.Check;
                case ("TRIGGER"):
                    return ConstraintType.Trigger;
                case ("DEFAULT"):
                    return ConstraintType.Default;
                case (""):
                    return null;
                default:
                    return null;
            }
        }
        
        private static bool GetBoolFromString(string input)
        {
            bool result;
            input = input.ToUpper();
            if (input.ToUpper() == "NO" || input == "FALSE")
            {
                result = false;
            }
            else if (input.ToUpper() == "YES" || input == "TRUE")
            {
                result = true;
            }
            else
            {
                throw new Exception();
            }

            return result;
        }
    }