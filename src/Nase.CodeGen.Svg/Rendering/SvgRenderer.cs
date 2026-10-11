
using System.Security;
using System.Text;
using Nase.Core.IntermediateRepresentation;
using Nase.CodeGen.Svg.Layout;

namespace Nase.CodeGen.Svg.Rendering;

public sealed class SvgRenderer
{
    private const double HeaderHeight = 40;
    private const double ColumnHeight = 28;
    private const double CornerRadius = 8;

    public string Render(DatabaseIR database, DiagramLayout layout)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(layout);

        var builder = new StringBuilder();

        builder.AppendLine(
            $"""<svg xmlns="http://www.w3.org/2000/svg" width="{layout.Width}" height="{layout.Height}" viewBox="0 0 {layout.Width} {layout.Height}">""");

        builder.AppendLine("""
            <style>
                .table-header { fill: #6D4AFF; }
                .table-body { fill: #FFFFFF; stroke: #D1D5DB; stroke-width: 1; }
                .table-title { fill: #FFFFFF; font: bold 14px sans-serif; }
                .column-text { fill: #1F2937; font: 12px sans-serif; }
                .primary-key { fill: #F59E0B; }
                .relation { fill: none; stroke: #64748B; stroke-width: 1.8; }
            </style>
            """);

        foreach (var foreignKey in database.ForeignKeys)
        {
            var source = layout.Tables.FirstOrDefault(table =>
                QualifiedName(table.SchemaName, table.TableName)
                    .Equals(foreignKey.SourceTable,
                        StringComparison.OrdinalIgnoreCase));

            var target = layout.Tables.FirstOrDefault(table =>
                QualifiedName(table.SchemaName, table.TableName)
                    .Equals(foreignKey.TargetTable,
                        StringComparison.OrdinalIgnoreCase));

            if (source is null || target is null)
                continue;

            var sourceX = source.X + source.Width / 2;
            var sourceY = source.Y + source.Height;
            var targetX = target.X + target.Width / 2;
            var targetY = target.Y;

            builder.AppendLine(
                $"""<path class="relation" d="M {sourceX} {sourceY} L {targetX} {targetY}" />""");
        }

        foreach (var table in database.Tables)
        {
            var tableLayout = layout.Tables.FirstOrDefault(item =>
                item.SchemaName.Equals(
                    table.SchemaName, StringComparison.OrdinalIgnoreCase) &&
                item.TableName.Equals(
                    table.Name, StringComparison.OrdinalIgnoreCase));

            if (tableLayout is null)
                continue;

            var x = tableLayout.X;
            var y = tableLayout.Y;
            var width = tableLayout.Width;
            var height = tableLayout.Height;

            builder.AppendLine(
                $"""<rect class="table-body" x="{x}" y="{y}" width="{width}" height="{height}" rx="{CornerRadius}" />""");

            builder.AppendLine(
                $"""<path class="table-header" d="M {x + CornerRadius} {y} H {x + width - CornerRadius} Q {x + width} {y} {x + width} {y + CornerRadius} V {y + HeaderHeight} H {x} V {y + CornerRadius} Q {x} {y} {x + CornerRadius} {y} Z" />""");

            builder.AppendLine(
                $"""<text class="table-title" x="{x + 12}" y="{y + 25}">{Escape(QualifiedName(table.SchemaName, table.Name))}</text>""");

            for (var i = 0; i < table.Columns.Count; i++)
            {
                var column = table.Columns[i];
                var columnY = y + HeaderHeight + (i * ColumnHeight);
                var isPrimaryKey = table.Keys.Any(key =>
                    key.Kind == KeyKind.Primary &&
                    key.Columns.Contains(
                        column.Name, StringComparer.OrdinalIgnoreCase));

                if (i > 0)
                {
                    builder.AppendLine(
                        $"""<line x1="{x}" y1="{columnY}" x2="{x + width}" y2="{columnY}" stroke="#E5E7EB" />""");
                }

                if (isPrimaryKey)
                {
                    builder.AppendLine(
                        $"""<text class="primary-key" x="{x + 10}" y="{columnY + 19}">PK</text>""");
                }

                var type = column.TypeParameters.Count > 0
                    ? $"{column.DataType}({string.Join(", ", column.TypeParameters)})"
                    : column.DataType;

                builder.AppendLine(
                    $"""<text class="column-text" x="{x + (isPrimaryKey ? 36 : 12)}" y="{columnY + 19}">{Escape($"{column.Name} : {type}")}</text>""");
            }
        }

        builder.AppendLine("</svg>");

        return builder.ToString();
    }

    private static string QualifiedName(string schema, string table) =>
        $"{schema}.{table}";

    private static string Escape(string value) =>
        SecurityElement.Escape(value) ?? string.Empty;
}
