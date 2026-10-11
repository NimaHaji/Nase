namespace Nase.CodeGen.Svg.Layout;

public sealed record TableLayout(
    string SchemaName,
    string TableName,
    double X,
    double Y,
    double Width,
    double Height);
