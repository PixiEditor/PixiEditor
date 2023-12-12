using System.IO;
using System.Text;

namespace PixiEditor.Models.IO.PaletteParsers.Support;

public enum AdobeColorSpace
{
    Rgb = 0,
    Hsb = 1,
    Cmyk = 2,
    Lab = 7,
    Grayscale = 8
}

public enum ACOFileVersion
{
    Version1 = 1,
    Version2
}

public static class AdobeFileSupport
{
    /// <summary>
    /// Reads a 16bit unsigned integer in big-endian format.
    /// </summary>
    /// <param name="stream">The stream to read the data from.</param>
    /// <returns>The unsigned 16bit integer cast to an <c>Int32</c>.</returns>
    public static int ReadInt16(Stream stream)
    {
        return (stream.ReadByte() << 8) | (stream.ReadByte() << 0);
    }

    /// <summary>
    /// Reads a 32bit unsigned integer in big-endian format.
    /// </summary>
    /// <param name="stream">The stream to read the data from.</param>
    /// <returns>The unsigned 32bit integer cast to an <c>Int32</c>.</returns>
    public static int ReadInt32(Stream stream)
    {
        // big endian conversion: http://stackoverflow.com/a/14401341/148962

        return ((byte)stream.ReadByte() << 24) | ((byte)stream.ReadByte() << 16) | ((byte)stream.ReadByte() << 8) | (byte)stream.ReadByte();
    }

    /// <summary>
    /// Reads a unicode string of the specified length.
    /// </summary>
    /// <param name="stream">The stream to read the data from.</param>
    /// <param name="length">The number of characters in the string.</param>
    /// <returns>The string read from the stream.</returns>
    public static string ReadString(Stream stream, int length)
    {
        byte[] buffer;

        buffer = new byte[length * 2];

        stream.Read(buffer, 0, buffer.Length);

        return Encoding.BigEndianUnicode.GetString(buffer);
    }

    /// <summary>
    /// Writes a 16bit unsigned integer in big-endian format.
    /// </summary>
    /// <param name="stream">The stream to write the data to.</param>
    /// <param name="value">The value to write</param>
    public static void WriteInt16(Stream stream, short value)
    {
        stream.WriteByte((byte)(value >> 8));
        stream.WriteByte((byte)(value >> 0));
    }

    /// <summary>
    /// Writes a 32bit unsigned integer in big-endian format.
    /// </summary>
    /// <param name="stream">The stream to write the data to.</param>
    /// <param name="value">The value to write</param>
    public static void WriteInt32(Stream stream, int value)
    {
        stream.WriteByte((byte)((value & 0xFF000000) >> 24));
        stream.WriteByte((byte)((value & 0x00FF0000) >> 16));
        stream.WriteByte((byte)((value & 0x0000FF00) >> 8));
        stream.WriteByte((byte)((value & 0x000000FF) >> 0));
    }

    public static void WriteString(Stream stream, string value)
    {
        stream.Write(Encoding.BigEndianUnicode.GetBytes(value), 0, value.Length * 2);
    }
}

