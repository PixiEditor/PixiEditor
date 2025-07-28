namespace PixiEditor.OperatingSystem;

public interface IEncryptor
{
    public byte[] Encrypt(byte[] data);
    public byte[] Decrypt(byte[] data);
}
