using System.IO;
using System.Text;
using PixiEditor.Extensions.Palettes;
using PixiEditor.Extensions.Palettes.Parsers;

namespace PixiEditor.Models.IO.PaletteParsers;

// Reads Microsoft RIFF palette data.

// There are two variants of RIFF palettes, simple and extended.
// This is implementation only supports the simple format with consists
// of RGB colors. I've yet to come across files using the extended variant, but
// if anyone comes across these I should be able to easily add with some testing.
// The extended variant includes extra header data describing how the palette
// is used, and can be either RGB, YUV or XYZ color data.
internal class MicrosoftPalParser : PaletteFileParser
{
    public override string FileName { get; } = "Microsoft RIFF Palette";
    public override string[] SupportedFileExtensions { get; } = new string[] { ".pal" };

    private static byte[] MsPalRiffSig = { (byte)'R', (byte)'I', (byte)'F', (byte)'F' };
    private static byte[] MsPalRiffType = { (byte)'P', (byte)'A', (byte)'L', (byte)' ' };
    private static byte[] MsPalRiffChunkSig = { (byte)'d', (byte)'a', (byte)'t', (byte)'a' };
    private static byte[] MsPalRiffChunkPalVer = { 0x00, 0x03 };

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
            using (BinaryReader br = new BinaryReader(stream))
            {
                // Read the file header.
                string riff = ReadByteString(br, 4); // RIFF type
                if (riff != "RIFF")
                    return PaletteFileData.Corrupted; // RIFF header not found

                int fileLength = br.ReadInt32();

                string riffType = ReadByteString(br, 4); // RIFF chunk signature
                if (riffType != "PAL ")
                    return PaletteFileData.Corrupted; // PAL header not found

                string chunkType = ReadByteString(br, 4); // data chunk signature
                if (chunkType != "data")
                    return PaletteFileData.Corrupted; // RGB palette not found

                br.ReadBytes(4); // Skip Chunk size
                br.ReadBytes(2); // Skip palette version
                int palCount = br.ReadInt16();

                for (int i = 0; i < palCount; i++)
                {
                    byte[] colorArray = br.ReadBytes(4);
                    colorPalette.Add(new PaletteColor(colorArray[0], colorArray[1], colorArray[2]));
                }
            }
        }

        return new PaletteFileData(name, colorPalette.ToArray());
    }

    public override async Task<bool> Save(string path, PaletteFileData data)
    {
        try
        {
            List<byte> byteList = new List<byte>();

            byteList.AddRange(MsPalRiffSig); // riff header
            byteList.AddRange(BitConverter.GetBytes(data.Colors.Length * 4 + 16)); // file size
            byteList.AddRange(MsPalRiffType); // form type
            byteList.AddRange(MsPalRiffChunkSig); // data chunk header
            byteList.AddRange(BitConverter.GetBytes(data.Colors.Length * 4 + 4)); // chunk size
            byteList.AddRange(MsPalRiffChunkPalVer); // pal version (always 03)
            byteList.AddRange(BitConverter.GetBytes((short)256)); // color count

            for (int i = 0; i < data.Colors.Length; i++)
            {
                byteList.Add(data.Colors[i].R);
                byteList.Add(data.Colors[i].G);
                byteList.Add(data.Colors[i].B);
                byteList.Add(0); // ignore alpha, as it's not used
            }

            await File.WriteAllBytesAsync(path, byteList.ToArray());

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string ReadByteString(BinaryReader br, int length)
    {
        return Encoding.ASCII.GetString(br.ReadBytes(length));
    }
}
