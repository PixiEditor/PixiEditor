using PixiEditor.Extensions.CommonApi.FlyUI;

namespace PixiEditor.Extensions.Wasm.Api.LayoutBuilding;

internal static class LayoutElementsStore
{
    public static Dictionary<int, ILayoutElement<CompiledControl>> LayoutElements { get; } = new();

    public static void AddElement(int internalId, ILayoutElement<CompiledControl> element)
    {
        LayoutElements.Add(internalId, element);
    }

    public static void RemoveElement(int internalId)
    {
        LayoutElements.Remove(internalId);
    }
}
