using Nase.Core.Syntax.AST;

namespace Nase.Core.Semantics;

public sealed class SemanticAnalyzer
{
    public SemanticAnalysisResult Analyze(SchemaNode schema)
    {
        var diagnostics = new List<SemanticDiagnostic>();

        var tableNames = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var table in schema.Tables)
        {
            var qualifiedName = GetQualifiedTableName(
                table.SchemaName,
                table.TableName);

            if (!tableNames.Add(qualifiedName))
            {
                diagnostics.Add(new SemanticDiagnostic(
                    "SEM001",
                    $"Table '{qualifiedName}' is defined more than once."));
            }
        }

        var tablesByName = schema.Tables
            .GroupBy(
                table => GetQualifiedTableName(
                    table.SchemaName,
                    table.TableName),
                StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.First(),
                StringComparer.OrdinalIgnoreCase);

        foreach (var table in schema.Tables)
        {
            ValidateConstraintColumns(table, diagnostics);

            ValidateForeignKeys(
                table,
                tablesByName,
                diagnostics);
        }

        return new SemanticAnalysisResult(
            diagnostics.Count == 0,
            diagnostics);
    }

    private static string GetQualifiedTableName(
        string? schemaName,
        string tableName)
    {
        return $"{schemaName ?? "dbo"}.{tableName}";
    }


    private static void ValidateForeignKeys(
        CreateTableNode table,
        IReadOnlyDictionary<string, CreateTableNode> tablesByName,
        ICollection<SemanticDiagnostic> diagnostics)
    {
        var constraints = table.Constraints.Concat(
            table.Columns.SelectMany(column => column.Constraints));

        foreach (var foreignKey in constraints.OfType<ForeignKeyConstraintNode>())
        {
            var referencedTableName = GetQualifiedTableName(
                foreignKey.ReferencedSchema,
                foreignKey.ReferencedTable);

            if (!tablesByName.TryGetValue(
                    referencedTableName,
                    out var referencedTable))
            {
                diagnostics.Add(new SemanticDiagnostic(
                    "SEM003",
                    $"Referenced table '{referencedTableName}' does not exist."));

                continue;
            }

            var referencedColumnNames = new HashSet<string>(
                referencedTable.Columns.Select(column => column.Name),
                StringComparer.OrdinalIgnoreCase);

            foreach (var columnName in foreignKey.ReferencedColumns)
            {
                if (!referencedColumnNames.Contains(columnName))
                {
                    diagnostics.Add(new SemanticDiagnostic(
                        "SEM004",
                        $"Referenced column '{columnName}' does not exist " +
                        $"in table '{referencedTableName}'."));
                }
            }

            var candidateKeys = GetCandidateKeys(referencedTable);

            var hasMatchingKey = candidateKeys.Any(key =>
                HaveSameColumns(key, foreignKey.ReferencedColumns));

            if (!hasMatchingKey)
            {
                diagnostics.Add(new SemanticDiagnostic(
                    "SEM005",
                    $"Referenced columns ({string.Join(", ", foreignKey.ReferencedColumns)}) " +
                    $"in table '{referencedTableName}' do not match a PRIMARY KEY or UNIQUE constraint."));
            }
        }
    }

    private static IEnumerable<IReadOnlyList<string>> GetCandidateKeys(
        CreateTableNode table)
    {
        var constraints = table.Constraints.Concat(
            table.Columns.SelectMany(column => column.Constraints));

        foreach (var constraint in constraints)
        {
            switch (constraint)
            {
                case PrimaryKeyConstraintNode primaryKey:
                    yield return primaryKey.Columns;
                    break;

                case UniqueConstraintNode unique:
                    yield return unique.Columns;
                    break;
            }
        }
    }

    private static bool HaveSameColumns(
        IReadOnlyList<string> first,
        IReadOnlyList<string> second)
    {
        return first.Count == second.Count &&
               first.ToHashSet(StringComparer.OrdinalIgnoreCase)
                   .SetEquals(second);
    }


    private static void ValidateConstraintColumns(
        CreateTableNode table,
        ICollection<SemanticDiagnostic> diagnostics)
    {
        var columnNames = new HashSet<string>(
            table.Columns.Select(column => column.Name),
            StringComparer.OrdinalIgnoreCase);

        var constraints = table.Constraints.Concat(
            table.Columns.SelectMany(column => column.Constraints));

        foreach (var constraint in constraints)
        {
            IEnumerable<string> referencedColumns = constraint switch
            {
                PrimaryKeyConstraintNode primaryKey => primaryKey.Columns,
                ForeignKeyConstraintNode foreignKey => foreignKey.Columns,
                UniqueConstraintNode unique => unique.Columns,
                _ => []
            };

            foreach (var columnName in referencedColumns)
            {
                if (!columnNames.Contains(columnName))
                {
                    diagnostics.Add(new SemanticDiagnostic(
                        "SEM002",
                        $"Column '{columnName}' referenced by constraint " +
                        $"'{constraint.Name ?? constraint.GetType().Name}' " +
                        $"does not exist in table " +
                        $"'{table.SchemaName ?? "dbo"}.{table.TableName}'."));
                }
            }
        }
    }
}
