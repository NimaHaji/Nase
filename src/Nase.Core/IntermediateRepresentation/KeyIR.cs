namespace Nase.Core.IntermediateRepresentation;

public sealed record KeyIR(
    string? Name,
    KeyKind Kind,
    IReadOnlyList<string> Columns);
