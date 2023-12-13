
namespace MigrationTool.Models;

public class Constraint
{
    public string Name { get; set; }
    public ConstraintType Type { get; set; }

    public static ConstraintType GetConstraintType(string? value)
    {
        return value switch
        {
            "PRIMARY KEY" => ConstraintType.PrimaryKey,
            "FOREIGN KEY" => ConstraintType.ForeignKey,
            "INDEX" => ConstraintType.Index,
            "UNIQUE" => ConstraintType.Unique,
            "CHECK" => ConstraintType.Check,
            "TRIGGER" => ConstraintType.Trigger,
            "DEFAULT" => ConstraintType.Default,
            _ => ConstraintType.Unknown
        };
    }

    public static string GetAnnotationForConstraint(ConstraintType type)
    {
        return type switch
        {
            ConstraintType.PrimaryKey => "[Key]",
            ConstraintType.ForeignKey => "[ForeignKey]",
            ConstraintType.Index => "[Index]",
            ConstraintType.Unique => "[Unique]",
            ConstraintType.Check => "[Check]",
            ConstraintType.Trigger => "[Trigger]",
            ConstraintType.Default => "[Default]"
        };
    }
}
