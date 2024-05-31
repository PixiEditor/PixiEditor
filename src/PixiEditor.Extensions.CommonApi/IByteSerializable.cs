namespace PixiEditor.Extensions.CommonApi;

public interface IByteSerializable
{
    byte[] Serialize();
    void Deserialize(byte[] data);
}
