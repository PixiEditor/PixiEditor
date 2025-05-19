using System.Collections.Immutable;

namespace PixiEditor.Extensions.FlyUI;

public interface IPropertyDeserializable
{
    public IEnumerable<object> GetProperties();
    public void DeserializeProperties(List<object> values);
}
