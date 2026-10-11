using Nase.Core.IntermediateRepresentation;
using Nase.CodeGen.Svg.Layout;
using Nase.CodeGen.Svg.Rendering;

namespace Nase.CodeGen.Svg;

public sealed class SvgGenerator
{
    private readonly DiagramLayoutEngine _layoutEngine = new();
    private readonly SvgRenderer _renderer = new();

    public string Generate(DatabaseIR database)
    {
        ArgumentNullException.ThrowIfNull(database);

        var layout = _layoutEngine.Calculate(database);

        return _renderer.Render(database, layout);
    }
}
