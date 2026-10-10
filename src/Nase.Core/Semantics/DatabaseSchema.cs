namespace Nase.Core.Semantics;

// Language-independent model used by later compiler stages.
public sealed record DatabaseSchema(
    IReadOnlyList<TableDefinition> Tables);

public sealed record TableDefinition(
    string Name,
    IReadOnlyList<ColumnDefinition> Columns);

public sealed record ColumnDefinition(
    string Name,
    string DataType,
    bool IsNullable);
