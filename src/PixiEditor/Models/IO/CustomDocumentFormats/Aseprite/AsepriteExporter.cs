using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace PixiEditor.Models.IO.CustomDocumentFormats.Aseprite;

/// <summary>
/// Writes an <see cref="AsepriteFile"/> to a stream in the Aseprite (.ase/.aseprite) binary format.
/// </summary>
public class AsepriteExporter
{
    /// <summary>
    /// Writes the given Aseprite file to the specified stream.
    /// </summary>
    public static void Write(Stream stream, AsepriteFile file)
    {
        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);

        // We write everything to a memory stream first to calculate the file size,
        // then write the actual data.
        using var bodyStream = new MemoryStream();
        using var bodyWriter = new BinaryWriter(bodyStream, Encoding.UTF8, true);

        WriteHeader(bodyWriter, file);
        WriteFrames(bodyWriter, file);

        // Now write to the actual stream with the correct file size
        bodyStream.Position = 0;
        byte[] bodyBytes = bodyStream.ToArray();

        // Patch the file size at bytes 0..3
        uint fileSize = (uint)bodyBytes.Length;
        bodyBytes[0] = (byte)(fileSize & 0xFF);
        bodyBytes[1] = (byte)((fileSize >> 8) & 0xFF);
        bodyBytes[2] = (byte)((fileSize >> 16) & 0xFF);
        bodyBytes[3] = (byte)((fileSize >> 24) & 0xFF);

        writer.Write(bodyBytes);
    }

    /// <summary>
    /// Writes the given Aseprite file to the specified file path.
    /// </summary>
    public static void Write(string path, AsepriteFile file)
    {
        using var stream = File.Create(path);
        Write(stream, file);
    }

    /// <summary>
    /// Creates an AsepriteFile from raw RGBA pixel data for each frame.
    /// Each frame's pixel data is expected in RGBA byte order (4 bytes per pixel).
    /// </summary>
    /// <param name="width">Image width in pixels.</param>
    /// <param name="height">Image height in pixels.</param>
    /// <param name="frames">List of (layerName, x, y, pixelData, duration) per frame per layer.</param>
    /// <param name="layers">Layer definitions (name, opacity, blendMode, isVisible, isGroup, childLevel).</param>
    /// <returns>A fully populated AsepriteFile.</returns>
    public static AsepriteFile CreateFromLayers(
        ushort width,
        ushort height,
        List<AsepriteLayerInfo> layers,
        List<AsepriteFrameInfo> frames)
    {
        var file = new AsepriteFile
        {
            Width = width,
            Height = height,
            ColorDepth = 32, // RGBA
            Flags = 1, // Layer opacity valid
            ColorCount = 0,
            PixelWidth = 1,
            PixelHeight = 1,
            GridWidth = 16,
            GridHeight = 16
        };

        // Build frames
        for (int f = 0; f < frames.Count; f++)
        {
            var frameInfo = frames[f];
            var frame = new AsepriteFrame
            {
                FrameDuration = frameInfo.DurationMs
            };

            // On first frame, add layer chunks
            if (f == 0)
            {
                for (int l = 0; l < layers.Count; l++)
                {
                    var layerInfo = layers[l];
                    var layerChunk = new AsepriteLayerChunk
                    {
                        ChunkType = 0x2004,
                        Flags = (ushort)(
                            (layerInfo.IsVisible ? 1 : 0) |
                            (layerInfo.IsEditable ? 2 : 0)),
                        LayerType = (ushort)(layerInfo.IsGroup ? 1 : 0),
                        ChildLevel = layerInfo.ChildLevel,
                        BlendMode = layerInfo.BlendMode,
                        Opacity = layerInfo.Opacity,
                        Name = layerInfo.Name ?? $"Layer {l}"
                    };
                    frame.Chunks.Add(layerChunk);
                }
            }

            // Add cel chunks for each layer's cel data in this frame
            if (frameInfo.Cels != null)
            {
                foreach (var cel in frameInfo.Cels)
                {
                    var celChunk = new AsepriteCelChunk
                    {
                        ChunkType = 0x2005,
                        LayerIndex = cel.LayerIndex,
                        X = cel.X,
                        Y = cel.Y,
                        Opacity = cel.Opacity,
                        CelType = cel.IsLinked ? (ushort)1 : (ushort)2,
                        ZIndex = 0,
                    };

                    if (cel.IsLinked)
                    {
                        celChunk.LinkedFramePosition = cel.LinkedFrame;
                    }
                    else
                    {
                        celChunk.Width = cel.Width;
                        celChunk.Height = cel.Height;
                        celChunk.CompressedPixelData = CompressPixelData(cel.PixelData);
                    }

                    frame.Chunks.Add(celChunk);
                }
            }

            file.Frames.Add(frame);
        }

        return file;
    }

    private static void WriteHeader(BinaryWriter writer, AsepriteFile file)
    {
        writer.Write((uint)0); // File size placeholder
        writer.Write(file.MagicNumber);
        writer.Write(file.FramesCount);
        writer.Write(file.Width);
        writer.Write(file.Height);
        writer.Write(file.ColorDepth);
        writer.Write(file.Flags);
        writer.Write(file.Speed);
        writer.Write((uint)0); // Set be 0
        writer.Write((uint)0); // Set be 0
        writer.Write(file.TransparentIndex);
        writer.Write(new byte[3]); // Padding
        writer.Write(file.ColorCount);
        writer.Write(file.PixelWidth);
        writer.Write(file.PixelHeight);
        writer.Write(file.GridX);
        writer.Write(file.GridY);
        writer.Write(file.GridWidth);
        writer.Write(file.GridHeight);
        writer.Write(new byte[84]); // Reserved
    }

    private static void WriteFrames(BinaryWriter writer, AsepriteFile file)
    {
        foreach (var frame in file.Frames)
        {
            long frameStartPos = writer.BaseStream.Position;
            writer.Write((uint)0); // Frame size placeholder
            writer.Write(frame.MagicNumber);

            uint chunkCount = (uint)frame.Chunks.Count;
            writer.Write(chunkCount <= 0xFFFF ? (ushort)chunkCount : (ushort)0xFFFF);
            writer.Write(frame.FrameDuration);
            writer.Write(new byte[2]); // Reserved
            writer.Write(chunkCount);

            foreach (var chunk in frame.Chunks)
            {
                WriteChunk(writer, chunk, file);
            }

            // Patch frame size
            long frameEndPos = writer.BaseStream.Position;
            uint frameSize = (uint)(frameEndPos - frameStartPos);
            writer.BaseStream.Position = frameStartPos;
            writer.Write(frameSize);
            writer.BaseStream.Position = frameEndPos;
        }
    }

    private static void WriteChunk(BinaryWriter writer, AsepriteChunk chunk, AsepriteFile file)
    {
        long chunkStartPos = writer.BaseStream.Position;
        writer.Write((uint)0); // Chunk size placeholder
        writer.Write(chunk.ChunkType);

        switch (chunk)
        {
            case AsepriteOldPaletteChunk0004 pal0004:
                WriteOldPaletteChunk0004(writer, pal0004);
                break;
            case AsepriteOldPaletteChunk0011 pal0011:
                WriteOldPaletteChunk0011(writer, pal0011);
                break;
            case AsepriteLayerChunk layer:
                WriteLayerChunk(writer, layer, file);
                break;
            case AsepriteCelChunk cel:
                WriteCelChunk(writer, cel);
                break;
            case AsepriteCelExtraChunk celExtra:
                WriteCelExtraChunk(writer, celExtra);
                break;
            case AsepriteColorProfileChunk colorProfile:
                WriteColorProfileChunk(writer, colorProfile);
                break;
            case AsepriteExternalFilesChunk extFiles:
                WriteExternalFilesChunk(writer, extFiles);
                break;
            case AsepriteMaskChunk mask:
                WriteMaskChunk(writer, mask);
                break;
            case AsepritePathChunk:
                // No data
                break;
            case AsepriteTagsChunk tags:
                WriteTagsChunk(writer, tags);
                break;
            case AsepritePaletteChunk palette:
                WritePaletteChunk(writer, palette);
                break;
            case AsepriteUserDataChunk userData:
                WriteUserDataChunk(writer, userData);
                break;
            case AsepriteSliceChunk slice:
                WriteSliceChunk(writer, slice);
                break;
            case AsepriteTilesetChunk tileset:
                WriteTilesetChunk(writer, tileset);
                break;
            case AsepriteRawChunk raw:
                if (raw.Data != null && raw.Data.Length > 0)
                    writer.Write(raw.Data);
                break;
        }

        // Patch chunk size
        long chunkEndPos = writer.BaseStream.Position;
        uint chunkSize = (uint)(chunkEndPos - chunkStartPos);
        writer.BaseStream.Position = chunkStartPos;
        writer.Write(chunkSize);
        writer.BaseStream.Position = chunkEndPos;
    }

    private static void WriteOldPaletteChunk0004(BinaryWriter writer, AsepriteOldPaletteChunk0004 chunk)
    {
        writer.Write((ushort)chunk.Packets.Count);
        foreach (var packet in chunk.Packets)
        {
            writer.Write(packet.Skip);
            writer.Write(packet.ColorCount);
            foreach (var color in packet.Colors)
            {
                writer.Write(color.R);
                writer.Write(color.G);
                writer.Write(color.B);
            }
        }
    }

    private static void WriteOldPaletteChunk0011(BinaryWriter writer, AsepriteOldPaletteChunk0011 chunk)
    {
        writer.Write((ushort)chunk.Packets.Count);
        foreach (var packet in chunk.Packets)
        {
            writer.Write(packet.Skip);
            writer.Write(packet.ColorCount);
            foreach (var color in packet.Colors)
            {
                writer.Write(color.R);
                writer.Write(color.G);
                writer.Write(color.B);
            }
        }
    }

    private static void WriteLayerChunk(BinaryWriter writer, AsepriteLayerChunk chunk, AsepriteFile file)
    {
        writer.Write(chunk.Flags);
        writer.Write(chunk.LayerType);
        writer.Write(chunk.ChildLevel);
        writer.Write(chunk.DefaultWidth);
        writer.Write(chunk.DefaultHeight);
        writer.Write(chunk.BlendMode);
        writer.Write(chunk.Opacity);
        writer.Write(new byte[3]); // Reserved
        WriteString(writer, chunk.Name ?? "");
        if (chunk.LayerType == 2)
            writer.Write(chunk.TilesetIndex);
        if (file.LayersHaveUuid && chunk.Uuid != null && chunk.Uuid.Length == 16)
            writer.Write(chunk.Uuid);
    }

    private static void WriteCelChunk(BinaryWriter writer, AsepriteCelChunk chunk)
    {
        writer.Write(chunk.LayerIndex);
        writer.Write(chunk.X);
        writer.Write(chunk.Y);
        writer.Write(chunk.Opacity);
        writer.Write(chunk.CelType);
        writer.Write(chunk.ZIndex);
        writer.Write(new byte[5]); // Reserved

        switch (chunk.CelType)
        {
            case 0: // Raw Image Data
                writer.Write(chunk.Width);
                writer.Write(chunk.Height);
                if (chunk.RawPixelData != null)
                    writer.Write(chunk.RawPixelData);
                break;
            case 1: // Linked Cel
                writer.Write(chunk.LinkedFramePosition);
                break;
            case 2: // Compressed Image
                writer.Write(chunk.Width);
                writer.Write(chunk.Height);
                if (chunk.CompressedPixelData != null)
                    writer.Write(chunk.CompressedPixelData);
                break;
            case 3: // Compressed Tilemap
                writer.Write(chunk.WidthInTiles);
                writer.Write(chunk.HeightInTiles);
                writer.Write(chunk.BitsPerTile);
                writer.Write(chunk.BitmaskTileId);
                writer.Write(chunk.BitmaskXFlip);
                writer.Write(chunk.BitmaskYFlip);
                writer.Write(chunk.BitmaskDiagonalFlip);
                writer.Write(new byte[10]); // Reserved
                if (chunk.CompressedTileData != null)
                    writer.Write(chunk.CompressedTileData);
                break;
        }
    }

    private static void WriteCelExtraChunk(BinaryWriter writer, AsepriteCelExtraChunk chunk)
    {
        writer.Write(chunk.Flags);
        writer.Write(chunk.PreciseX);
        writer.Write(chunk.PreciseY);
        writer.Write(chunk.WidthInSprite);
        writer.Write(chunk.HeightInSprite);
        writer.Write(new byte[16]); // Reserved
    }

    private static void WriteColorProfileChunk(BinaryWriter writer, AsepriteColorProfileChunk chunk)
    {
        writer.Write(chunk.Type);
        writer.Write(chunk.Flags);
        writer.Write(chunk.FixedGamma);
        writer.Write(new byte[8]); // Reserved
        if (chunk.Type == 2 && chunk.ICCProfileData != null)
        {
            writer.Write((uint)chunk.ICCProfileData.Length);
            writer.Write(chunk.ICCProfileData);
        }
    }

    private static void WriteExternalFilesChunk(BinaryWriter writer, AsepriteExternalFilesChunk chunk)
    {
        writer.Write((uint)chunk.Entries.Count);
        writer.Write(new byte[8]); // Reserved
        foreach (var entry in chunk.Entries)
        {
            writer.Write(entry.EntryId);
            writer.Write(entry.Type);
            writer.Write(new byte[7]); // Reserved
            WriteString(writer, entry.ExternalFileNameOrExtensionId ?? "");
        }
    }

    private static void WriteMaskChunk(BinaryWriter writer, AsepriteMaskChunk chunk)
    {
        writer.Write(chunk.X);
        writer.Write(chunk.Y);
        writer.Write(chunk.Width);
        writer.Write(chunk.Height);
        writer.Write(new byte[8]); // Reserved
        WriteString(writer, chunk.Name ?? "");
        if (chunk.BitMapData != null)
            writer.Write(chunk.BitMapData);
    }

    private static void WriteTagsChunk(BinaryWriter writer, AsepriteTagsChunk chunk)
    {
        writer.Write((ushort)chunk.Tags.Count);
        writer.Write(new byte[8]); // Reserved
        foreach (var tag in chunk.Tags)
        {
            writer.Write(tag.FromFrame);
            writer.Write(tag.ToFrame);
            writer.Write(tag.LoopAnimationDirection);
            writer.Write(tag.RepeatNTimes);
            writer.Write(new byte[6]); // Reserved
            writer.Write(tag.TagColor ?? new byte[3], 0, 3);
            writer.Write((byte)0); // Extra byte
            WriteString(writer, tag.TagName ?? "");
        }
    }

    private static void WritePaletteChunk(BinaryWriter writer, AsepritePaletteChunk chunk)
    {
        writer.Write(chunk.NewPaletteSize);
        writer.Write(chunk.FirstColorIndex);
        writer.Write(chunk.LastColorIndex);
        writer.Write(new byte[8]); // Reserved
        foreach (var entry in chunk.Entries)
        {
            writer.Write(entry.Flags);
            writer.Write(entry.R);
            writer.Write(entry.G);
            writer.Write(entry.B);
            writer.Write(entry.A);
            if (entry.HasName)
                WriteString(writer, entry.Name ?? "");
        }
    }

    private static void WriteUserDataChunk(BinaryWriter writer, AsepriteUserDataChunk chunk)
    {
        writer.Write(chunk.Flags);
        if (chunk.HasText)
            WriteString(writer, chunk.Text ?? "");
        if (chunk.HasColor)
            writer.Write(chunk.Color ?? new byte[4], 0, 4);
        if (chunk.HasProperties)
        {
            // We need to calculate the total size of the property maps data.
            // For simplicity, write to a temp buffer first.
            using var propStream = new MemoryStream();
            using var propWriter = new BinaryWriter(propStream, Encoding.UTF8, true);

            propWriter.Write((uint)chunk.PropertyMaps.Count);
            foreach (var map in chunk.PropertyMaps)
            {
                propWriter.Write(map.Key);
                propWriter.Write((uint)map.Properties.Count);
                foreach (var prop in map.Properties)
                {
                    WriteString(propWriter, prop.Name ?? "");
                    propWriter.Write(prop.Type);
                    // For simplicity, we skip writing complex property values
                    // and just write what we can. Full round-trip of properties
                    // would require a more complete implementation.
                }
            }

            byte[] propData = propStream.ToArray();
            // +4 for the size field itself
            writer.Write((uint)(propData.Length + 4));
            writer.Write(propData);
        }
    }

    private static void WriteSliceChunk(BinaryWriter writer, AsepriteSliceChunk chunk)
    {
        writer.Write((uint)chunk.SliceKeys.Count);
        writer.Write(chunk.Flags);
        writer.Write((uint)0); // Reserved
        WriteString(writer, chunk.Name ?? "");
        foreach (var key in chunk.SliceKeys)
        {
            writer.Write(key.FrameNumber);
            writer.Write(key.SliceX);
            writer.Write(key.SliceY);
            writer.Write(key.SliceWidth);
            writer.Write(key.SliceHeight);
            if (chunk.IsNinePatch)
            {
                writer.Write(key.CenterX);
                writer.Write(key.CenterY);
                writer.Write(key.CenterWidth);
                writer.Write(key.CenterHeight);
            }
            if (chunk.HasPivot)
            {
                writer.Write(key.PivotX);
                writer.Write(key.PivotY);
            }
        }
    }

    private static void WriteTilesetChunk(BinaryWriter writer, AsepriteTilesetChunk chunk)
    {
        writer.Write(chunk.TilesetId);
        writer.Write(chunk.Flags);
        writer.Write(chunk.NumberOfTiles);
        writer.Write(chunk.TileWidth);
        writer.Write(chunk.TileHeight);
        writer.Write(chunk.BaseIndex);
        writer.Write(new byte[14]); // Reserved
        WriteString(writer, chunk.Name ?? "");
        if (chunk.HasExternalFileLink)
        {
            writer.Write(chunk.ExternalFileId);
            writer.Write(chunk.TilesetIdInExternalFile);
        }
        if (chunk.HasTilesInside && chunk.CompressedTilesetImage != null)
        {
            writer.Write((uint)chunk.CompressedTilesetImage.Length);
            writer.Write(chunk.CompressedTilesetImage);
        }
    }

    private static void WriteString(BinaryWriter writer, string value)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(value);
        writer.Write((ushort)bytes.Length);
        writer.Write(bytes);
    }

    /// <summary>
    /// Compresses raw pixel data using ZLIB (deflate with zlib header).
    /// </summary>
    public static byte[] CompressPixelData(byte[] rawData)
    {
        if (rawData == null || rawData.Length == 0)
            return Array.Empty<byte>();

        using var output = new MemoryStream();
        // ZLIB header
        output.WriteByte(0x78); // CMF: CM=8 (deflate), CINFO=7 (32K window)
        output.WriteByte(0x9C); // FLG: FCHECK, FLEVEL=2 (default)

        using (var deflate = new DeflateStream(output, CompressionLevel.Optimal, true))
        {
            deflate.Write(rawData, 0, rawData.Length);
        }

        // Adler32 checksum
        uint adler = ComputeAdler32(rawData);
        output.WriteByte((byte)((adler >> 24) & 0xFF));
        output.WriteByte((byte)((adler >> 16) & 0xFF));
        output.WriteByte((byte)((adler >> 8) & 0xFF));
        output.WriteByte((byte)(adler & 0xFF));

        return output.ToArray();
    }

    /// <summary>
    /// Decompresses ZLIB-compressed pixel data.
    /// </summary>
    public static byte[] DecompressPixelData(byte[] compressedData)
    {
        if (compressedData == null || compressedData.Length == 0)
            return Array.Empty<byte>();

        // Skip ZLIB header (2 bytes)
        using var input = new MemoryStream(compressedData, 2, compressedData.Length - 2);
        using var deflate = new DeflateStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        deflate.CopyTo(output);
        return output.ToArray();
    }

    private static uint ComputeAdler32(byte[] data)
    {
        const uint MOD_ADLER = 65521;
        uint a = 1, b = 0;
        for (int i = 0; i < data.Length; i++)
        {
            a = (a + data[i]) % MOD_ADLER;
            b = (b + a) % MOD_ADLER;
        }
        return (b << 16) | a;
    }
}

/// <summary>
/// Layer metadata for constructing an AsepriteFile from document layers.
/// </summary>
public class AsepriteLayerInfo
{
    public string Name { get; set; }
    public byte Opacity { get; set; } = 255;
    public ushort BlendMode { get; set; } = 0;
    public bool IsVisible { get; set; } = true;
    public bool IsEditable { get; set; } = true;
    public bool IsGroup { get; set; }
    public ushort ChildLevel { get; set; }
}

/// <summary>
/// Frame metadata for constructing an AsepriteFile.
/// </summary>
public class AsepriteFrameInfo
{
    public ushort DurationMs { get; set; } = 100;
    public List<AsepriteCelInfo> Cels { get; set; } = new();
}

/// <summary>
/// Cel (cell) data for a single layer in a single frame.
/// </summary>
public class AsepriteCelInfo
{
    public ushort LayerIndex { get; set; }
    public short X { get; set; }
    public short Y { get; set; }
    public byte Opacity { get; set; } = 255;
    public ushort Width { get; set; }
    public ushort Height { get; set; }
    /// <summary>Raw RGBA pixel data (4 bytes per pixel, row by row).</summary>
    public byte[] PixelData { get; set; }
    public bool IsLinked { get; set; }
    public ushort LinkedFrame { get; set; }
}
