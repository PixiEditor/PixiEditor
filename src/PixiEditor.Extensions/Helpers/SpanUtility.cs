using System.Runtime.InteropServices;
using System.Text;

namespace PixiEditor.Extensions.Helpers;

public static class SpanUtility
{
    public static object? Read(Type type, Span<byte> span, ref int offset)
    {
        if (type == typeof(string))
        {
            int stringLength = BitConverter.ToInt32(span[offset..(offset + sizeof(int))]);
            offset += sizeof(int);
            return Encoding.UTF8.GetString(span[offset..(offset + stringLength)]);
        }

        if (type == typeof(int))
        {
            int value = BitConverter.ToInt32(span[offset..(offset + sizeof(int))]);
            offset += sizeof(int);
            return value;
        }
        if (type == typeof(bool))
        {
            bool value = BitConverter.ToBoolean(span[offset..(offset + sizeof(bool))]);
            offset += sizeof(bool);
            return value;
        }

        if (type == typeof(byte))
        {
            byte value = span[offset];
            offset++;
            return value;
        }

        if (type == typeof(float))
        {
            float value = BitConverter.ToSingle(span[offset..(offset + sizeof(float))]);
            offset += sizeof(float);
            return value;
        }

        if (type == typeof(double))
        {
            double value = BitConverter.ToDouble(span[offset..(offset + sizeof(double))]);
            offset += sizeof(double);
            return value;
        }

        return Marshal.PtrToStructure(span[offset..(offset + Marshal.SizeOf(type))].GetPinnableReference(), type);
    }
}
