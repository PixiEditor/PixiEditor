namespace PixiEditor.OperatingSystem.Cryptography;

public class AesHmacEncryptor : IEncryptor
{
    private string password;

    public AesHmacEncryptor(string pwd)
    {
        password = pwd;
    }

    public byte[] Encrypt(byte[] data)
    {
        return AesThenHmac.SimpleEncryptWithPassword(data, password);
    }

    public byte[] Decrypt(byte[] data)
    {
        return AesThenHmac.SimpleDecryptWithPassword(data, password);
    }
}
