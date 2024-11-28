namespace PixiEditor.Extensions.CommonApi.FlyUI.Properties;

public interface IStructProperty
{
    public byte[] Serialize();
    public void Deserialize(byte[] data);
}
