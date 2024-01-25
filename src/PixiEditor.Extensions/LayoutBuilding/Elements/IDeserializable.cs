using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.LayoutBuilding.Elements;

public interface IDeserializable
{
    public void DeserializeProperties(List<object> values);
}
