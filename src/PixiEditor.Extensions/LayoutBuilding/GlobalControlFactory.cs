using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;
using PixiEditor.Extensions.LayoutBuilding.Elements;
using Button = PixiEditor.Extensions.LayoutBuilding.Elements.Button;

namespace PixiEditor.Extensions.LayoutBuilding;

public static class GlobalControlFactory
{
    public static IReadOnlyDictionary<int, Func<ILayoutElement<Control>>> Map => map;

    private static Dictionary<int, Func<ILayoutElement<Control>>> map = new()
    {
        { 0, () => new Layout() },
        { 1,  () => new Center() },
        { 2, () => new Text() },
        { 3, () => new Button() },
        { 4, () => new StatefulContainer() }
    };
}
