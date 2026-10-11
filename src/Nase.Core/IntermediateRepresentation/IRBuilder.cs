
using Nase.Core.Semantics;
using Nase.Core.Syntax.AST;

namespace Nase.Core.IntermediateRepresentation;

public sealed class IRBuilder
{
    public DatabaseIR Build(
        SchemaNode schema,
        SemanticAnalysisResult analysisResult)
    {
        if (!analysisResult.IsValid)
        {
            throw new InvalidOperationException(
                "Cannot build IR because semantic analysis failed.");
        }

        var tables = schema.Tables
            .Select(BuildTable)
            .ToArray();

        var foreignKeys = schema.Tables
            .SelectMany(table =>
                GetConstraints(table)
                    .OfType<ForeignKeyConstraintNode>()
                    .Select(foreignKey => new ForeignKeyIR(
                        foreignKey.Name,
                        GetQualifiedTableName(
                            table.SchemaName,
                            table.TableName),
                        foreignKey.Columns.ToArray(),
                        GetQualifiedTableName(
                            foreignKey.ReferencedSchema,
                            foreignKey.ReferencedTable),
                        foreignKey.ReferencedColumns.ToArray())))
            .ToArray();

        return new DatabaseIR(tables, foreignKeys);
    }

    private static TableIR BuildTable(CreateTableNode table)
    {
        var columns = table.Columns
            .Select(column => new ColumnIR(
                column.Name,
                column.DataType.Name,
                column.DataType.Parameters.ToArray(),
                column.IsNullable))
            .ToArray();

        var keys = GetConstraints(table)
            .Select(constraint => constraint switch
            {
                PrimaryKeyConstraintNode primaryKey =>
                    new KeyIR(
                        primaryKey.Name,
                        KeyKind.Primary,
                        primaryKey.Columns.ToArray()),

                UniqueConstraintNode unique =>
                    new KeyIR(
                        unique.Name,
                        KeyKind.Unique,
                        unique.Columns.ToArray()),

                _ => null
            })
            .OfType<KeyIR>()
            .ToArray();

        return new TableIR(
            table.SchemaName ?? "dbo",
            table.TableName,
            columns,
            keys);
    }

    private static IEnumerable<ConstraintNode> GetConstraints(
        CreateTableNode table)
    {
        return table.Constraints.Concat(
            table.Columns.SelectMany(column => column.Constraints));
    }

    private static string GetQualifiedTableName(
        string? schemaName,
        string tableName)
    {
        return $"{schemaName ?? "dbo"}.{tableName}";
    }
}
