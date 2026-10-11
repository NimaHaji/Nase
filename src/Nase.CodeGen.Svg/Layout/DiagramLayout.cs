namespace Nase.CodeGen.Svg.Layout;

public sealed record DiagramLayout(
    IReadOnlyList<TableLayout> Tables,
    double Width,
    double Height);
