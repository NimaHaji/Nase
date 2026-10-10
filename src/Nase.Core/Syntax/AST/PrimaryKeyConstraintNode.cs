namespace Nase.Core.Syntax.AST;

public sealed record PrimaryKeyConstraintNode(
    string? Name,
    IReadOnlyList<string> Columns)
    : ConstraintNode(Name);
