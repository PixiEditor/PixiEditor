using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Extensions.CommonApi.Palettes.Parsers;

namespace PixiEditor.Models.IO.PaletteParsers;

/// <summary>
/// Reads 8-bit color palette data from Interleaved Bitmap format (LBM/BBM)
/// most commonly used by DeluxePaint on Amiga and MS DOS.
///
/// https://en.wikipedia.org/wiki/ILBM
///
/// Note: A BBM file is essentially a LBM without a full image.
/// 
/// Code adapted from https://devblog.cyotek.com/post/loading-the-color-palette-from-a-bbm-lbm-image-file-using-csharp
/// </summary>
internal class DeluxePaintParser : PaletteFileParser
{
    public override string FileName { get; } = "DeluxePaint Interleaved Bitmap Palette";
    public override string[] SupportedFileExtensions { get; } = new string[] { ".bbm", ".lbm" };
    public override async Task<PaletteFileData> Parse(string path)
    {
        try
        {
            return await ParseFile(path);
        }
        catch
        {
            return PaletteFileData.Corrupted;
        }
    }

    private static async Task<PaletteFileData> ParseFile(string path)
    {
        List<PaletteColor> colorPalette = new();
        string name = Path.GetFileNameWithoutExtension(path);

        await using (Stream stream = File.OpenRead(path))
        {
            byte[] buffer;
            string header;

            // read the FORM header that identifies the document as an IFF file
            buffer = new byte[4];
            stream.Read(buffer, 0, buffer.Length);
            if (Encoding.ASCII.GetString(buffer) != "FORM")
                return PaletteFileData.Corrupted; // Form header not found

            // the next value is the size of all the data in the FORM chunk
            // We don't actually need this value, but we have to read it
            // regardless to advance the stream
            ReadInt(stream);

            stream.Read(buffer, 0, buffer.Length);
            header = Encoding.ASCII.GetString(buffer);
            if (header != "PBM " && header != "ILBM")
                return PaletteFileData.Corrupted; // Bitmap header not found

            while (stream.Read(buffer, 0, buffer.Length) == buffer.Length)
            {
                int chunkLength;

                chunkLength = ReadInt(stream);

                if (Encoding.ASCII.GetString(buffer) != "CMAP")
                {
                    // some other LBM chunk, skip it
                    if (stream.CanSeek)
                    {
                        stream.Seek(chunkLength, SeekOrigin.Current);
                    }
                    else
                    {
                        for (int i = 0; i < chunkLength; i++)
                            stream.ReadByte();
                    }
                }
                else
                {
                    // color map chunk
                    for (int i = 0; i < chunkLength / 3; i++)
                    {
                        int[] rgb = new int[3];

                        rgb[0] = stream.ReadByte();
                        rgb[1] = stream.ReadByte();
                        rgb[2] = stream.ReadByte();

                        colorPalette.Add(new PaletteColor((byte)rgb[0], (byte)rgb[1], (byte)rgb[2]));
                    }

                    // all done so stop reading the rest of the file
                    break;
                }

                // chunks always contain an even number of bytes even if the recorded length is odd
                // if the length is odd, then there's a padding byte in the file - just read and discard
                if (chunkLength % 2 != 0)
                    stream.ReadByte();
            }
        }

        return new PaletteFileData(name, colorPalette.ToArray());
    }

    public override bool CanSave => false;

    public override async Task<bool> Save(string path, PaletteFileData data)
    {
        throw new SavingNotSupportedException("Saving palette as .bbm or .lbm is not supported.");
    }

    private static int ReadInt(Stream stream)
    {
        byte[] buffer;

        // big endian conversion: http://stackoverflow.com/a/14401341/148962

        buffer = new byte[4];
        stream.Read(buffer, 0, buffer.Length);

        return (buffer[0] << 24) | (buffer[1] << 16) | (buffer[2] << 8) | buffer[3];
    }
}
