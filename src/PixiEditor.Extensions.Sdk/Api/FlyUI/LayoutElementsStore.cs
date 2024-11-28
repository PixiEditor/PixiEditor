using PixiEditor.Extensions.CommonApi.FlyUI;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

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
