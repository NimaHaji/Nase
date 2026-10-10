namespace Nase.Core.Syntax;

// Syntax-level models. These represent what the parser reads.
public sealed record SchemaSyntax(
    IReadOnlyList<TableSyntax> Tables);

public sealed record TableSyntax(
    string Name,
    IReadOnlyList<ColumnSyntax> Columns);

public sealed record ColumnSyntax(
    string Name,
    string TypeName,
    bool IsNullable);
