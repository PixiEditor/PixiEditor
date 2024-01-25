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

        return Marshal.PtrToStructure(span[offset..(offset + Marshal.SizeOf(type))].GetPinnableReference(), type);
    }
}
