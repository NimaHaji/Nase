namespace Nase.Core.Syntax.AST;

public sealed record UniqueConstraintNode(
    string? Name,
    IReadOnlyList<string> Columns)
    : ConstraintNode(Name);
