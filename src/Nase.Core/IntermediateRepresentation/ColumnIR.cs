namespace Nase.Core.IntermediateRepresentation;

public sealed record ColumnIR(
    string Name,
    string DataType,
    IReadOnlyList<int> TypeParameters,
    bool IsNullable);