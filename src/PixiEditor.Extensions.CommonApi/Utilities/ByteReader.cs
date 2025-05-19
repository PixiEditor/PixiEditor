namespace PixiEditor.Extensions.CommonApi.Utilities;

public class ByteReader
{
    private byte[] _buffer;
    private int _position;

    public ByteReader(byte[] buffer)
    {
        _buffer = buffer;
        _position = 0;
    }

    public byte ReadByte()
    {
        return _buffer[_position++];
    }

    public string ReadString()
    {
        int length = ReadInt();
        string result = System.Text.Encoding.UTF8.GetString(_buffer, _position, length);
        _position += length;
        return result;
    }

    public int ReadInt()
    {
        int result = BitConverter.ToInt32(_buffer, _position);
        _position += sizeof(int);
        return result;
    }

    public float ReadFloat()
    {
        float result = BitConverter.ToSingle(_buffer, _position);
        _position += sizeof(float);
        return result;
    }

    public bool ReadBool()
    {
        bool result = BitConverter.ToBoolean(_buffer, _position);
        _position += sizeof(bool);
        return result;
    }

    public byte[] ReadBytes(int length)
    {
        byte[] result = new byte[length];
        Array.Copy(_buffer, _position, result, 0, length);
        _position += length;
        return result;
    }

    public double ReadDouble()
    {
        double result = BitConverter.ToDouble(_buffer, _position);
        _position += sizeof(double);
        return result;
    }
}
