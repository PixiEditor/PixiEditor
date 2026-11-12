using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PixiEditor.Models.IO.CustomDocumentFormats.Aseprite;

/// <summary>
/// Reads a .ase/.aseprite file from a stream into an <see cref="AsepriteFile"/> object,
/// preserving all information from every chunk type.
/// </summary>
public class AsepriteImporter
{
    /// <summary>
    /// Reads an Aseprite file from the given stream.
    /// </summary>
    public static AsepriteFile Read(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.UTF8, true);
        var file = new AsepriteFile();

        // --- Header (128 bytes) ---
        file.FileSize = reader.ReadUInt32();
        file.MagicNumber = reader.ReadUInt16();
        if (file.MagicNumber != 0xA5E0)
            throw new InvalidDataException("Invalid Aseprite magic number (expected 0xA5E0).");

        ushort framesCount = reader.ReadUInt16();
        file.Width = reader.ReadUInt16();
        file.Height = reader.ReadUInt16();
        file.ColorDepth = reader.ReadUInt16();
        file.Flags = reader.ReadUInt32();
        file.Speed = reader.ReadUInt16();
        reader.ReadUInt32(); // Set be 0
        reader.ReadUInt32(); // Set be 0
        file.TransparentIndex = reader.ReadByte();
        reader.ReadBytes(3); // Ignore
        file.ColorCount = reader.ReadUInt16();
        file.PixelWidth = reader.ReadByte();
        file.PixelHeight = reader.ReadByte();
        file.GridX = reader.ReadInt16();
        file.GridY = reader.ReadInt16();
        file.GridWidth = reader.ReadUInt16();
        file.GridHeight = reader.ReadUInt16();
        reader.ReadBytes(84); // Reserved for future

        // --- Frames ---
        for (int i = 0; i < framesCount; i++)
        {
            var frame = new AsepriteFrame();
            long frameStartPos = stream.Position;
            frame.BytesInFrame = reader.ReadUInt32();
            frame.MagicNumber = reader.ReadUInt16();
            if (frame.MagicNumber != 0xF1FA)
                throw new InvalidDataException("Invalid frame magic number (expected 0xF1FA).");

            ushort oldChunkCount = reader.ReadUInt16();
            frame.FrameDuration = reader.ReadUInt16();
            reader.ReadBytes(2); // Reserved
            uint newChunkCount = reader.ReadUInt32();
            uint chunkCount = newChunkCount == 0 ? oldChunkCount : newChunkCount;

            long frameEnd = frameStartPos + frame.BytesInFrame;
            if (frame.BytesInFrame == 0) frameEnd = stream.Length;

            for (uint c = 0; c < chunkCount; c++)
            {
                if (stream.Position >= stream.Length) break;

                uint chunkSize = reader.ReadUInt32();
                ushort chunkType = reader.ReadUInt16();
                long chunkEndPos = stream.Position + chunkSize - 6;

                AsepriteChunk chunk = ParseChunk(reader, chunkType, chunkEndPos, file);
                chunk.ChunkSize = chunkSize;
                chunk.ChunkType = chunkType;
                frame.Chunks.Add(chunk);

                // Ensure we're at the correct position for the next chunk
                if (stream.Position != chunkEndPos && chunkEndPos <= stream.Length)
                    stream.Position = chunkEndPos;
            }

            file.Frames.Add(frame);
            // Ensure we advance to the end of the frame if needed
            if (frame.BytesInFrame > 0 && stream.Position < frameEnd)
                stream.Position = frameEnd;
        }

        return file;
    }

    /// <summary>
    /// Reads an Aseprite file from the given file path.
    /// </summary>
    public static AsepriteFile Read(string path)
    {
        using var stream = File.OpenRead(path);
        return Read(stream);
    }

    private static AsepriteChunk ParseChunk(BinaryReader reader, ushort chunkType, long chunkEndPos, AsepriteFile file)
    {
        switch (chunkType)
        {
            case 0x0004: return ParseOldPaletteChunk0004(reader);
            case 0x0011: return ParseOldPaletteChunk0011(reader);
            case 0x2004: return ParseLayerChunk(reader, file);
            case 0x2005: return ParseCelChunk(reader, chunkEndPos);
            case 0x2006: return ParseCelExtraChunk(reader);
            case 0x2007: return ParseColorProfileChunk(reader);
            case 0x2008: return ParseExternalFilesChunk(reader);
            case 0x2016: return ParseMaskChunk(reader);
            case 0x2017: return new AsepritePathChunk();
            case 0x2018: return ParseTagsChunk(reader);
            case 0x2019: return ParsePaletteChunk(reader);
            case 0x2020: return ParseUserDataChunk(reader, chunkEndPos);
            case 0x2022: return ParseSliceChunk(reader);
            case 0x2023: return ParseTilesetChunk(reader);
            default:
                var raw = new AsepriteRawChunk();
                int size = (int)(chunkEndPos - reader.BaseStream.Position);
                raw.Data = size > 0 ? reader.ReadBytes(size) : Array.Empty<byte>();
                return raw;
        }
    }

    private static AsepriteOldPaletteChunk0004 ParseOldPaletteChunk0004(BinaryReader reader)
    {
        var chunk = new AsepriteOldPaletteChunk0004();
        ushort packets = reader.ReadUInt16();
        for (int i = 0; i < packets; i++)
        {
            var p = new AsepriteOldPaletteChunk0004.Packet();
            p.Skip = reader.ReadByte();
            p.ColorCount = reader.ReadByte();
            int count = p.ColorCount == 0 ? 256 : p.ColorCount;
            for (int c = 0; c < count; c++)
            {
                p.Colors.Add(new AsepriteOldPaletteChunk0004.Color
                {
                    R = reader.ReadByte(),
                    G = reader.ReadByte(),
                    B = reader.ReadByte()
                });
            }
            chunk.Packets.Add(p);
        }
        return chunk;
    }

    private static AsepriteOldPaletteChunk0011 ParseOldPaletteChunk0011(BinaryReader reader)
    {
        var chunk = new AsepriteOldPaletteChunk0011();
        ushort packets = reader.ReadUInt16();
        for (int i = 0; i < packets; i++)
        {
            var p = new AsepriteOldPaletteChunk0011.Packet();
            p.Skip = reader.ReadByte();
            p.ColorCount = reader.ReadByte();
            int count = p.ColorCount == 0 ? 256 : p.ColorCount;
            for (int c = 0; c < count; c++)
            {
                p.Colors.Add(new AsepriteOldPaletteChunk0011.Color
                {
                    R = reader.ReadByte(),
                    G = reader.ReadByte(),
                    B = reader.ReadByte()
                });
            }
            chunk.Packets.Add(p);
        }
        return chunk;
    }

    private static AsepriteLayerChunk ParseLayerChunk(BinaryReader reader, AsepriteFile file)
    {
        var chunk = new AsepriteLayerChunk();
        chunk.Flags = reader.ReadUInt16();
        chunk.LayerType = reader.ReadUInt16();
        chunk.ChildLevel = reader.ReadUInt16();
        chunk.DefaultWidth = reader.ReadUInt16();
        chunk.DefaultHeight = reader.ReadUInt16();
        chunk.BlendMode = reader.ReadUInt16();
        chunk.Opacity = reader.ReadByte();
        reader.ReadBytes(3); // Reserved
        chunk.Name = ReadString(reader);
        if (chunk.LayerType == 2)
            chunk.TilesetIndex = reader.ReadUInt32();
        // Read UUID if header flags bit 4 is set
        if (file.LayersHaveUuid)
            chunk.Uuid = reader.ReadBytes(16);
        return chunk;
    }

    private static AsepriteCelChunk ParseCelChunk(BinaryReader reader, long chunkEndPos)
    {
        var chunk = new AsepriteCelChunk();
        chunk.LayerIndex = reader.ReadUInt16();
        chunk.X = reader.ReadInt16();
        chunk.Y = reader.ReadInt16();
        chunk.Opacity = reader.ReadByte();
        chunk.CelType = reader.ReadUInt16();
        chunk.ZIndex = reader.ReadInt16();
        reader.ReadBytes(5); // Reserved

        switch (chunk.CelType)
        {
            case 0: // Raw Image Data
                chunk.Width = reader.ReadUInt16();
                chunk.Height = reader.ReadUInt16();
                int rawSize = (int)(chunkEndPos - reader.BaseStream.Position);
                chunk.RawPixelData = reader.ReadBytes(rawSize);
                break;
            case 1: // Linked Cel
                chunk.LinkedFramePosition = reader.ReadUInt16();
                break;
            case 2: // Compressed Image
                chunk.Width = reader.ReadUInt16();
                chunk.Height = reader.ReadUInt16();
                int compressedSize = (int)(chunkEndPos - reader.BaseStream.Position);
                chunk.CompressedPixelData = reader.ReadBytes(compressedSize);
                break;
            case 3: // Compressed Tilemap
                chunk.WidthInTiles = reader.ReadUInt16();
                chunk.HeightInTiles = reader.ReadUInt16();
                chunk.BitsPerTile = reader.ReadUInt16();
                chunk.BitmaskTileId = reader.ReadUInt32();
                chunk.BitmaskXFlip = reader.ReadUInt32();
                chunk.BitmaskYFlip = reader.ReadUInt32();
                chunk.BitmaskDiagonalFlip = reader.ReadUInt32();
                reader.ReadBytes(10); // Reserved
                int tileSize = (int)(chunkEndPos - reader.BaseStream.Position);
                chunk.CompressedTileData = reader.ReadBytes(tileSize);
                break;
        }

        return chunk;
    }

    private static AsepriteCelExtraChunk ParseCelExtraChunk(BinaryReader reader)
    {
        var chunk = new AsepriteCelExtraChunk();
        chunk.Flags = reader.ReadUInt32();
        chunk.PreciseX = reader.ReadUInt32();
        chunk.PreciseY = reader.ReadUInt32();
        chunk.WidthInSprite = reader.ReadUInt32();
        chunk.HeightInSprite = reader.ReadUInt32();
        reader.ReadBytes(16); // Reserved
        return chunk;
    }

    private static AsepriteColorProfileChunk ParseColorProfileChunk(BinaryReader reader)
    {
        var chunk = new AsepriteColorProfileChunk();
        chunk.Type = reader.ReadUInt16();
        chunk.Flags = reader.ReadUInt16();
        chunk.FixedGamma = reader.ReadUInt32();
        reader.ReadBytes(8); // Reserved
        if (chunk.Type == 2) // ICC
        {
            uint len = reader.ReadUInt32();
            chunk.ICCProfileData = reader.ReadBytes((int)len);
        }
        return chunk;
    }

    private static AsepriteExternalFilesChunk ParseExternalFilesChunk(BinaryReader reader)
    {
        var chunk = new AsepriteExternalFilesChunk();
        uint count = reader.ReadUInt32();
        reader.ReadBytes(8); // Reserved
        for (int i = 0; i < count; i++)
        {
            var entry = new AsepriteExternalFilesChunk.Entry();
            entry.EntryId = reader.ReadUInt32();
            entry.Type = reader.ReadByte();
            reader.ReadBytes(7); // Reserved
            entry.ExternalFileNameOrExtensionId = ReadString(reader);
            chunk.Entries.Add(entry);
        }
        return chunk;
    }

    private static AsepriteMaskChunk ParseMaskChunk(BinaryReader reader)
    {
        var chunk = new AsepriteMaskChunk();
        chunk.X = reader.ReadInt16();
        chunk.Y = reader.ReadInt16();
        chunk.Width = reader.ReadUInt16();
        chunk.Height = reader.ReadUInt16();
        reader.ReadBytes(8); // Reserved
        chunk.Name = ReadString(reader);
        int size = chunk.Height * ((chunk.Width + 7) / 8);
        chunk.BitMapData = reader.ReadBytes(size);
        return chunk;
    }

    private static AsepriteTagsChunk ParseTagsChunk(BinaryReader reader)
    {
        var chunk = new AsepriteTagsChunk();
        // TODO: Map Aseprite tags to PixiEditor animation tags/markers
        ushort count = reader.ReadUInt16();
        reader.ReadBytes(8); // Reserved
        for (int i = 0; i < count; i++)
        {
            var tag = new AsepriteTagsChunk.Tag();
            tag.FromFrame = reader.ReadUInt16();
            tag.ToFrame = reader.ReadUInt16();
            tag.LoopAnimationDirection = reader.ReadByte();
            tag.RepeatNTimes = reader.ReadUInt16();
            reader.ReadBytes(6); // Reserved
            tag.TagColor = reader.ReadBytes(3);
            reader.ReadByte(); // Extra byte
            tag.TagName = ReadString(reader);
            chunk.Tags.Add(tag);
        }
        return chunk;
    }

    private static AsepritePaletteChunk ParsePaletteChunk(BinaryReader reader)
    {
        var chunk = new AsepritePaletteChunk();
        chunk.NewPaletteSize = reader.ReadUInt32();
        chunk.FirstColorIndex = reader.ReadUInt32();
        chunk.LastColorIndex = reader.ReadUInt32();
        reader.ReadBytes(8); // Reserved
        for (uint i = chunk.FirstColorIndex; i <= chunk.LastColorIndex; i++)
        {
            var entry = new AsepritePaletteChunk.Entry();
            entry.Flags = reader.ReadUInt16();
            entry.R = reader.ReadByte();
            entry.G = reader.ReadByte();
            entry.B = reader.ReadByte();
            entry.A = reader.ReadByte();
            if (entry.HasName)
                entry.Name = ReadString(reader);
            chunk.Entries.Add(entry);
        }
        return chunk;
    }

    private static AsepriteUserDataChunk ParseUserDataChunk(BinaryReader reader, long chunkEndPos)
    {
        var chunk = new AsepriteUserDataChunk();
        chunk.Flags = reader.ReadUInt32();
        if (chunk.HasText)
            chunk.Text = ReadString(reader);
        if (chunk.HasColor)
            chunk.Color = reader.ReadBytes(4);
        if (chunk.HasProperties)
        {
            uint totalSize = reader.ReadUInt32(); // Size of all property maps data
            uint mapCount = reader.ReadUInt32();
            for (uint m = 0; m < mapCount; m++)
            {
                var map = new AsepriteUserDataChunk.PropertyMap();
                map.Key = reader.ReadUInt32();
                uint propCount = reader.ReadUInt32();
                for (uint p = 0; p < propCount; p++)
                {
                    var prop = new AsepriteUserDataChunk.Property();
                    prop.Name = ReadString(reader);
                    prop.Type = reader.ReadUInt16();
                    prop.Value = ReadPropertyValue(reader, prop.Type, chunkEndPos);
                    map.Properties.Add(prop);
                }
                chunk.PropertyMaps.Add(map);
            }
        }
        return chunk;
    }

    private static object ReadPropertyValue(BinaryReader reader, ushort type, long chunkEndPos)
    {
        switch (type)
        {
            case 0x0001: return reader.ReadByte() != 0; // bool
            case 0x0002: return (sbyte)reader.ReadByte(); // int8
            case 0x0003: return reader.ReadByte(); // uint8
            case 0x0004: return reader.ReadInt16(); // int16
            case 0x0005: return reader.ReadUInt16(); // uint16
            case 0x0006: return reader.ReadInt32(); // int32
            case 0x0007: return reader.ReadUInt32(); // uint32
            case 0x0008: return reader.ReadInt64(); // int64
            case 0x0009: return reader.ReadUInt64(); // uint64
            case 0x000A: return reader.ReadUInt32(); // FIXED
            case 0x000B: return reader.ReadSingle(); // float
            case 0x000C: return reader.ReadDouble(); // double
            case 0x000D: return ReadString(reader); // string
            case 0x000E: return new int[] { reader.ReadInt32(), reader.ReadInt32() }; // POINT
            case 0x000F: return new int[] { reader.ReadInt32(), reader.ReadInt32() }; // SIZE
            case 0x0010: return new int[] { reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32() }; // RECT
            case 0x0011: // vector
            {
                uint elemCount = reader.ReadUInt32();
                ushort elemType = reader.ReadUInt16();
                var list = new List<object>();
                for (uint i = 0; i < elemCount; i++)
                {
                    ushort actualType = elemType == 0 ? reader.ReadUInt16() : elemType;
                    list.Add(ReadPropertyValue(reader, actualType, chunkEndPos));
                }
                return list;
            }
            case 0x0012: // nested properties map
            {
                uint propCount = reader.ReadUInt32();
                var nested = new List<AsepriteUserDataChunk.Property>();
                for (uint i = 0; i < propCount; i++)
                {
                    var prop = new AsepriteUserDataChunk.Property();
                    prop.Name = ReadString(reader);
                    prop.Type = reader.ReadUInt16();
                    prop.Value = ReadPropertyValue(reader, prop.Type, chunkEndPos);
                    nested.Add(prop);
                }
                return nested;
            }
            case 0x0013: return reader.ReadBytes(16); // UUID
            default:
                // Unknown type - skip to chunk end
                long remaining = chunkEndPos - reader.BaseStream.Position;
                if (remaining > 0) reader.ReadBytes((int)remaining);
                return null;
        }
    }

    private static AsepriteSliceChunk ParseSliceChunk(BinaryReader reader)
    {
        var chunk = new AsepriteSliceChunk();
        uint count = reader.ReadUInt32();
        chunk.Flags = reader.ReadUInt32();
        reader.ReadUInt32(); // Reserved
        chunk.Name = ReadString(reader);
        for (int i = 0; i < count; i++)
        {
            var key = new AsepriteSliceChunk.SliceKey();
            key.FrameNumber = reader.ReadUInt32();
            key.SliceX = reader.ReadInt32();
            key.SliceY = reader.ReadInt32();
            key.SliceWidth = reader.ReadUInt32();
            key.SliceHeight = reader.ReadUInt32();
            if (chunk.IsNinePatch)
            {
                key.CenterX = reader.ReadInt32();
                key.CenterY = reader.ReadInt32();
                key.CenterWidth = reader.ReadUInt32();
                key.CenterHeight = reader.ReadUInt32();
            }
            if (chunk.HasPivot)
            {
                key.PivotX = reader.ReadInt32();
                key.PivotY = reader.ReadInt32();
            }
            chunk.SliceKeys.Add(key);
        }
        return chunk;
    }

    private static AsepriteTilesetChunk ParseTilesetChunk(BinaryReader reader)
    {
        var chunk = new AsepriteTilesetChunk();
        chunk.TilesetId = reader.ReadUInt32();
        chunk.Flags = reader.ReadUInt32();
        chunk.NumberOfTiles = reader.ReadUInt32();
        chunk.TileWidth = reader.ReadUInt16();
        chunk.TileHeight = reader.ReadUInt16();
        chunk.BaseIndex = reader.ReadInt16();
        reader.ReadBytes(14); // Reserved
        chunk.Name = ReadString(reader);
        if (chunk.HasExternalFileLink)
        {
            chunk.ExternalFileId = reader.ReadUInt32();
            chunk.TilesetIdInExternalFile = reader.ReadUInt32();
        }
        if (chunk.HasTilesInside)
        {
            uint dataLength = reader.ReadUInt32();
            chunk.CompressedTilesetImage = reader.ReadBytes((int)dataLength);
        }
        return chunk;
    }

    private static string ReadString(BinaryReader reader)
    {
        ushort length = reader.ReadUInt16();
        byte[] bytes = reader.ReadBytes(length);
        return Encoding.UTF8.GetString(bytes);
    }
}
