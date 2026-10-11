namespace Nase.Core.IntermediateRepresentation;

public sealed record TableIR(
    string SchemaName,
    string Name,
    IReadOnlyList<ColumnIR> Columns,
    IReadOnlyList<KeyIR> Keys);
