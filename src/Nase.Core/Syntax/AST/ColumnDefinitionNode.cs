namespace Nase.Core.Syntax.AST;

public sealed record ColumnDefinitionNode(
    string Name,
    DataTypeNode DataType,
    bool IsNullable,
    IReadOnlyList<ConstraintNode> Constraints);
