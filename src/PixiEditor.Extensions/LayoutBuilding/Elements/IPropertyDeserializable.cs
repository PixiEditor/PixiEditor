using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.LayoutBuilding.Elements;

public interface IPropertyDeserializable
{
    public IEnumerable<object> GetProperties();
    public void DeserializeProperties(IEnumerable<object> values);
}
