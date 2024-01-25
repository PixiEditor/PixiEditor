using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;
using PixiEditor.Extensions.LayoutBuilding.Elements;

namespace PixiEditor.Extensions.LayoutBuilding;

public static class GlobalControlFactory
{
    public static IReadOnlyDictionary<int, Func<IDeserializable>> Map => map;

    private static Dictionary<int, Func<IDeserializable>> map = new()
    {
        { 0, () => new Layout() },
        { 1,  () => new Center() },
        { 2, () => new Text() }
    };
}
