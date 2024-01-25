using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.LayoutBuilding.Elements;

public interface IPropertyDeserializable
{
    public void DeserializeProperties(List<object> values);
}
