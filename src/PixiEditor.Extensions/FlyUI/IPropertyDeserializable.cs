namespace PixiEditor.Extensions.FlyUI;

public interface IPropertyDeserializable
{
    public IEnumerable<object> GetProperties();
    public void DeserializeProperties(IEnumerable<object> values);
}
