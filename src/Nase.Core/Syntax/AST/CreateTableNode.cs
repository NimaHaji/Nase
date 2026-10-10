namespace Nase.Core.Syntax.AST;

public sealed record CreateTableNode(
    string? SchemaName,
    string TableName,
    IReadOnlyList<ColumnDefinitionNode> Columns,
    IReadOnlyList<ConstraintNode> Constraints);
