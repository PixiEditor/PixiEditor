namespace PixiEditor.Extensions.CommonApi.Utilities;

public class ByteWriter
{
    private List<byte> _buffer;
    private int _position;

    public ByteWriter()
    {
        _buffer = new List<byte>();
        _position = 0;
    }

    public void WriteByte(byte value)
    {
        _buffer.Add(value);
        _position++;
    }

    public void WriteString(string value)
    {
        byte[] stringBytes = System.Text.Encoding.UTF8.GetBytes(value);
        WriteInt(stringBytes.Length);
        _buffer.AddRange(stringBytes);
    }

    public void WriteInt(int value)
    {
        byte[] intBytes = BitConverter.GetBytes(value);
        _buffer.AddRange(intBytes);
    }

    public void WriteFloat(float value)
    {
        byte[] floatBytes = BitConverter.GetBytes(value);
        _buffer.AddRange(floatBytes);
    }

    public void WriteDouble(double value)
    {
        byte[] doubleBytes = BitConverter.GetBytes(value);
        _buffer.AddRange(doubleBytes);
    }

    public void WriteBool(bool value)
    {
        byte[] boolBytes = BitConverter.GetBytes(value);
        _buffer.AddRange(boolBytes);
    }

    public void WriteBytes(byte[] value)
    {
        _buffer.AddRange(value);
    }

    public byte[] ToArray()
    {
        return _buffer.ToArray();
    }
}
