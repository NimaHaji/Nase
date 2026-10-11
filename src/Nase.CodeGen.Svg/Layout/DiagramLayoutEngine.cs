
using Nase.Core.IntermediateRepresentation;

namespace Nase.CodeGen.Svg.Layout;

public sealed class DiagramLayoutEngine
{
    private const double Margin = 30;
    private const double TableWidth = 240;
    private const double HeaderHeight = 40;
    private const double ColumnHeight = 28;
    private const double HorizontalGap = 60;
    private const double VerticalGap = 60;
    private const int TablesPerRow = 3;

    public DiagramLayout Calculate(DatabaseIR database)
    {
        ArgumentNullException.ThrowIfNull(database);

        if (database.Tables.Count == 0)
        {
            return new DiagramLayout([], Margin * 2, Margin * 2);
        }

        var layouts = new List<TableLayout>();
        var rowHeights = database.Tables
            .Chunk(TablesPerRow)
            .Select(row => row.Max(table =>
                HeaderHeight + table.Columns.Count * ColumnHeight))
            .ToArray();

        var currentY = Margin;

        for (var rowIndex = 0; rowIndex < rowHeights.Length; rowIndex++)
        {
            var rowTables = database.Tables
                .Skip(rowIndex * TablesPerRow)
                .Take(TablesPerRow)
                .ToArray();

            for (var columnIndex = 0;
                 columnIndex < rowTables.Length;
                 columnIndex++)
            {
                var table = rowTables[columnIndex];

                var height = HeaderHeight +
                             table.Columns.Count * ColumnHeight;

                layouts.Add(new TableLayout(
                    table.SchemaName,
                    table.Name,
                    Margin + columnIndex * (TableWidth + HorizontalGap),
                    currentY,
                    TableWidth,
                    height));
            }

            currentY += rowHeights[rowIndex] + VerticalGap;
        }

        var totalWidth = Margin * 2 +
                         Math.Min(TablesPerRow, database.Tables.Count) *
                         TableWidth +
                         (Math.Min(TablesPerRow, database.Tables.Count) - 1) *
                         HorizontalGap;

        var totalHeight = currentY - VerticalGap + Margin;

        return new DiagramLayout(layouts, totalWidth, totalHeight);
    }
}
