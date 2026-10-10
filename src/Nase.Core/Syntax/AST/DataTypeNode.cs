namespace Nase.Core.Syntax.AST;

public sealed record DataTypeNode(
    string Name,
    IReadOnlyList<int> Parameters);
