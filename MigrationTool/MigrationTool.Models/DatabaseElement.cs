namespace MigrationTool.Models;

public class DatabaseElement
{
    public string TableName { get; set; }
    public string ColumnName { get; set; }
    public string DataType { get; set; }
    public int? MaxLength { get; set; }
    public bool IsNullable { get; set; }
    public string ForeignKeyConstraintName { get; set; }
    public string ForeignKeyTableName { get; set; }
    public string ForeignKeyColumnName { get; set; }
}