namespace Nase.Core.Syntax.AST;

public sealed record ForeignKeyConstraintNode(
    string? Name,
    IReadOnlyList<string> Columns,
    string? ReferencedSchema,
    string ReferencedTable,
    IReadOnlyList<string> ReferencedColumns)
    : ConstraintNode(Name);
