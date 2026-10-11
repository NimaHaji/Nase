
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
            <defs>
                <marker id="arrow"
                        markerWidth="7"
                        markerHeight="7"
                        refX="5"
                        refY="3"
                        orient="auto">
                    <path d="M0,0 L6,3 L0,6 Z" fill="#A78BFA"/>
                </marker>
            </defs>

            <rect width="100%" height="100%" fill="#212121"/>

            <style>
                .table-body {
                    fill: #2F2F2F;
                    stroke: #454545;
                    stroke-width: 1;
                }

                .table-header {
                    fill: #343039;
                }

                .table-title {
                    fill: #F4F4F5;
                    font: 600 14px sans-serif;
                }

                .column-text {
                    fill: #E4E4E7;
                    font: 12px sans-serif;
                }

                .column-type {
                    fill: #A1A1AA;
                    font: 12px sans-serif;
                }

                .primary-key {
                    fill: #C4B5FD;
                    font: bold 10px sans-serif;
                }

                .relation {
                    fill: none;
                    stroke: #A78BFA;
                    stroke-width: 1.6;
                    stroke-linecap: round;
                    stroke-linejoin: round;
                    opacity: 0.85;
                }

                .separator {
                    stroke: #454545;
                    stroke-width: 1;
                }
            </style>
            """);

        // Draw relationships behind the table cards.
        foreach (var foreignKey in database.ForeignKeys)
        {
            var source = layout.Tables.FirstOrDefault(table =>
                QualifiedName(table.SchemaName, table.TableName)
                    .Equals(
                        foreignKey.SourceTable,
                        StringComparison.OrdinalIgnoreCase));

            var target = layout.Tables.FirstOrDefault(table =>
                QualifiedName(table.SchemaName, table.TableName)
                    .Equals(
                        foreignKey.TargetTable,
                        StringComparison.OrdinalIgnoreCase));

            if (source is null || target is null)
                continue;

            var sourceX = source.X + source.Width / 2;
            var sourceY = source.Y + source.Height;
            var targetX = target.X + target.Width / 2;
            var targetY = target.Y;

            builder.AppendLine(
                $"""<path class="relation" marker-end="url(#arrow)" d="M {sourceX} {sourceY} L {sourceX} {(sourceY + targetY) / 2} L {targetX} {(sourceY + targetY) / 2} L {targetX} {targetY}" />""");
        }

        foreach (var table in database.Tables)
        {
            var tableLayout = layout.Tables.FirstOrDefault(item =>
                item.SchemaName.Equals(
                    table.SchemaName,
                    StringComparison.OrdinalIgnoreCase) &&
                item.TableName.Equals(
                    table.Name,
                    StringComparison.OrdinalIgnoreCase));

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
                var columnY = y + HeaderHeight + i * ColumnHeight;

                var isPrimaryKey = table.Keys.Any(key =>
                    key.Kind == KeyKind.Primary &&
                    key.Columns.Contains(
                        column.Name,
                        StringComparer.OrdinalIgnoreCase));

                var isUnique = table.Keys.Any(key =>
                    key.Kind == KeyKind.Unique &&
                    key.Columns.Contains(
                        column.Name,
                        StringComparer.OrdinalIgnoreCase));

                if (i > 0)
                {
                    builder.AppendLine(
                        $"""<line class="separator" x1="{x}" y1="{columnY}" x2="{x + width}" y2="{columnY}" />""");
                }

                var badge = isPrimaryKey ? "PK" : isUnique ? "UQ" : null;

                if (badge is not null)
                {
                    builder.AppendLine(
                        $"""<text class="primary-key" x="{x + 10}" y="{columnY + 18}">{badge}</text>""");
                }

                var type = column.TypeParameters.Count > 0
                    ? $"{column.DataType}({string.Join(", ", column.TypeParameters)})"
                    : column.DataType;

                var textX = x + (badge is null ? 12 : 36);

                builder.AppendLine(
                    $"""<text class="column-text" x="{textX}" y="{columnY + 18}">{Escape(column.Name)}</text>""");

                var typeX = x + width - 12;

                builder.AppendLine(
                    $"""<text class="column-type" x="{typeX}" y="{columnY + 18}" text-anchor="end">{Escape(type)}</text>""");
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
