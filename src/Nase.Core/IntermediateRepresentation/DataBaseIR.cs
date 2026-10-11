namespace Nase.Core.IntermediateRepresentation;

public sealed record DatabaseIR(
    IReadOnlyList<TableIR> Tables,
    IReadOnlyList<ForeignKeyIR> ForeignKeys);
