namespace Nase.Core.IntermediateRepresentation;

public sealed record ForeignKeyIR(
    string? Name,
    string SourceTable,
    IReadOnlyList<string> SourceColumns,
    string TargetTable,
    IReadOnlyList<string> TargetColumns);
