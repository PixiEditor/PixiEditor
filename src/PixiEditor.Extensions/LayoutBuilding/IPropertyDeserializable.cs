namespace PixiEditor.Extensions.LayoutBuilding;

public interface IPropertyDeserializable
{
    public IEnumerable<object> GetProperties();
    public void DeserializeProperties(IEnumerable<object> values);
}
