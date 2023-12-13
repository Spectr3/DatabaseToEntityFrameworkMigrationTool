namespace MigrationTool.Models;

public enum ConstraintType
{
    ForeignKey,
    Index,
    PrimaryKey,
    Unique,
    Check,
    Trigger,
    Default,
    Unknown
}