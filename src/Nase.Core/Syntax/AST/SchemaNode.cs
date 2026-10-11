namespace Nase.Core.Syntax.AST;

public sealed record SchemaNode(
    IReadOnlyList<CreateTableNode> Tables);
