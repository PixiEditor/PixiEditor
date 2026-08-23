using System;
using System.Collections.Generic;

namespace PixiEditor.Models.IO.CustomDocumentFormats.Aseprite;

/// <summary>
/// Represents a complete Aseprite (.ase/.aseprite) file. A lot of this data isn't actually used; It is still kept for completeness.
/// </summary>
public class AsepriteFile
{
    // --- Header (128 bytes) ---
    public uint FileSize { get; set; }
    public ushort MagicNumber { get; set; } = 0xA5E0;
    public ushort FramesCount => (ushort)Frames.Count;
    public ushort Width { get; set; }
    public ushort Height { get; set; }
    /// <summary>
    /// Color depth in bits per pixel: 32 = RGBA, 16 = Grayscale, 8 = Indexed.
    /// </summary>
    public ushort ColorDepth { get; set; }
    /// <summary>
    /// Flags:
    /// 1 = Layer opacity has valid value,
    /// 2 = Layer blend mode/opacity is valid for groups,
    /// 4 = Layers have a UUID.
    /// </summary>
    public uint Flags { get; set; }
    /// <summary>Deprecated speed field (milliseconds between frames).</summary>
    public ushort Speed { get; set; }
    /// <summary>Palette entry index representing transparent color (Indexed sprites only).</summary>
    public byte TransparentIndex { get; set; }
    /// <summary>Number of colors (0 means 256 for old sprites).</summary>
    public ushort ColorCount { get; set; }
    /// <summary>Pixel width for pixel ratio (pixel ratio = PixelWidth / PixelHeight). 0 means 1:1.</summary>
    public byte PixelWidth { get; set; }
    /// <summary>Pixel height for pixel ratio. 0 means 1:1.</summary>
    public byte PixelHeight { get; set; }
    public short GridX { get; set; }
    public short GridY { get; set; }
    /// <summary>Grid width (0 if no grid, default 16).</summary>
    public ushort GridWidth { get; set; }
    /// <summary>Grid height (0 if no grid, default 16).</summary>
    public ushort GridHeight { get; set; }
    // 84 bytes reserved (not stored)

    public bool LayerOpacityValid => (Flags & 1) != 0;
    public bool GroupBlendModeValid => (Flags & 2) != 0;
    public bool LayersHaveUuid => (Flags & 4) != 0;

    /// <summary>Number of bytes per pixel based on ColorDepth.</summary>
    public int BytesPerPixel => ColorDepth switch
    {
        32 => 4,
        16 => 2,
        8 => 1,
        _ => 4
    };

    public List<AsepriteFrame> Frames { get; set; } = new();
}

public class AsepriteFrame
{
    public uint BytesInFrame { get; set; }
    public ushort MagicNumber { get; set; } = 0xF1FA;
    /// <summary>Frame duration in milliseconds.</summary>
    public ushort FrameDuration { get; set; }
    public List<AsepriteChunk> Chunks { get; set; } = new();
}

public abstract class AsepriteChunk
{
    public uint ChunkSize { get; set; }
    public ushort ChunkType { get; set; }
}

/// <summary>Old palette chunk (0x0004). Colors are 0-255 range. Deleted if new palette chunk is found.</summary>
public class AsepriteOldPaletteChunk0004 : AsepriteChunk
{
    public class Packet
    {
        public byte Skip { get; set; }
        /// <summary>Number of colors in this packet (0 means 256).</summary>
        public byte ColorCount { get; set; }
        public List<Color> Colors { get; set; } = new();
    }
    public class Color
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
    }
    public List<Packet> Packets { get; set; } = new();
}

/// <summary>Old palette chunk (0x0011). Colors are 0-63 range. Deleted if new palette chunk is found.</summary>
public class AsepriteOldPaletteChunk0011 : AsepriteChunk
{
    public class Packet
    {
        public byte Skip { get; set; }
        /// <summary>Number of colors in this packet (0 means 256).</summary>
        public byte ColorCount { get; set; }
        public List<Color> Colors { get; set; } = new();
    }
    public class Color
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
    }
    public List<Packet> Packets { get; set; } = new();
}

/// <summary>Layer chunk (0x2004).</summary>
public class AsepriteLayerChunk : AsepriteChunk
{
    /// <summary>
    /// Flags: 1=Visible, 2=Editable, 4=Lock movement, 8=Background,
    /// 16=Prefer linked cels, 32=Collapsed, 64=Reference layer.
    /// </summary>
    public ushort Flags { get; set; }
    /// <summary>0=Normal, 1=Group, 2=Tilemap.</summary>
    public ushort LayerType { get; set; }
    public ushort ChildLevel { get; set; }
    public ushort DefaultWidth { get; set; }  // Ignored
    public ushort DefaultHeight { get; set; } // Ignored
    /// <summary>Blend mode (see spec NOTE.6).</summary>
    public ushort BlendMode { get; set; }
    public byte Opacity { get; set; }
    public string Name { get; set; }
    /// <summary>Tileset index (only if LayerType == 2).</summary>
    public uint TilesetIndex { get; set; }
    /// <summary>Layer UUID (16 bytes, only if header Flags bit 4 is set).</summary>
    public byte[] Uuid { get; set; }

    public bool IsVisible => (Flags & 1) != 0;
    public bool IsEditable => (Flags & 2) != 0;
    public bool IsLockMovement => (Flags & 4) != 0;
    public bool IsBackground => (Flags & 8) != 0;
    public bool PreferLinkedCels => (Flags & 16) != 0;
    public bool IsCollapsed => (Flags & 32) != 0;
    public bool IsReferenceLayer => (Flags & 64) != 0;
    public bool IsGroup => LayerType == 1;
    public bool IsTilemap => LayerType == 2;
}

/// <summary>Cel chunk (0x2005).</summary>
public class AsepriteCelChunk : AsepriteChunk
{
    public ushort LayerIndex { get; set; }
    public short X { get; set; }
    public short Y { get; set; }
    public byte Opacity { get; set; }
    /// <summary>
    /// 0=Raw Image Data, 1=Linked Cel, 2=Compressed Image, 3=Compressed Tilemap.
    /// </summary>
    public ushort CelType { get; set; }
    /// <summary>Z-Index offset for rendering order (see spec NOTE.5).</summary>
    public short ZIndex { get; set; }

    // For CelType 0 (Raw Image Data)
    public ushort Width { get; set; }
    public ushort Height { get; set; }
    public byte[] RawPixelData { get; set; }

    // For CelType 1 (Linked Cel)
    public ushort LinkedFramePosition { get; set; }

    // For CelType 2 (Compressed Image) - Width & Height reused from above
    public byte[] CompressedPixelData { get; set; }

    // For CelType 3 (Compressed Tilemap)
    public ushort WidthInTiles { get; set; }
    public ushort HeightInTiles { get; set; }
    public ushort BitsPerTile { get; set; }
    public uint BitmaskTileId { get; set; }
    public uint BitmaskXFlip { get; set; }
    public uint BitmaskYFlip { get; set; }
    public uint BitmaskDiagonalFlip { get; set; }
    public byte[] CompressedTileData { get; set; }
}

/// <summary>Cel Extra chunk (0x2006).</summary>
public class AsepriteCelExtraChunk : AsepriteChunk
{
    /// <summary>Flags: 1=Precise bounds are set.</summary>
    public uint Flags { get; set; }
    /// <summary>Precise X position (FIXED 16.16).</summary>
    public uint PreciseX { get; set; }
    /// <summary>Precise Y position (FIXED 16.16).</summary>
    public uint PreciseY { get; set; }
    /// <summary>Width in sprite, scaled (FIXED 16.16).</summary>
    public uint WidthInSprite { get; set; }
    /// <summary>Height in sprite, scaled (FIXED 16.16).</summary>
    public uint HeightInSprite { get; set; }
}

/// <summary>Color Profile chunk (0x2007).</summary>
public class AsepriteColorProfileChunk : AsepriteChunk
{
    /// <summary>0=No profile, 1=sRGB, 2=Embedded ICC.</summary>
    public ushort Type { get; set; }
    /// <summary>Flags: 1=Use special fixed gamma.</summary>
    public ushort Flags { get; set; }
    /// <summary>Fixed gamma (FIXED 16.16, 1.0 = linear).</summary>
    public uint FixedGamma { get; set; }
    /// <summary>ICC profile data (only when Type == 2).</summary>
    public byte[] ICCProfileData { get; set; }
}

/// <summary>External Files chunk (0x2008).</summary>
public class AsepriteExternalFilesChunk : AsepriteChunk
{
    public class Entry
    {
        public uint EntryId { get; set; }
        /// <summary>0=External palette, 1=External tileset, 2=Extension name for properties, 3=Extension name for tile management.</summary>
        public byte Type { get; set; }
        public string ExternalFileNameOrExtensionId { get; set; }
    }
    public List<Entry> Entries { get; set; } = new();
}

/// <summary>Mask chunk (0x2016) - DEPRECATED.</summary>
public class AsepriteMaskChunk : AsepriteChunk
{
    public short X { get; set; }
    public short Y { get; set; }
    public ushort Width { get; set; }
    public ushort Height { get; set; }
    public string Name { get; set; }
    /// <summary>Bit map data (size = height*((width+7)/8)).</summary>
    public byte[] BitMapData { get; set; }
}

/// <summary>Path chunk (0x2017) - Never used.</summary>
public class AsepritePathChunk : AsepriteChunk
{
}

/// <summary>Tags chunk (0x2018).</summary>
public class AsepriteTagsChunk : AsepriteChunk
{
    public class Tag
    {
        public ushort FromFrame { get; set; }
        public ushort ToFrame { get; set; }
        /// <summary>0=Forward, 1=Reverse, 2=Ping-pong, 3=Ping-pong Reverse.</summary>
        public byte LoopAnimationDirection { get; set; }
        /// <summary>Repeat count (0=unspecified/infinite).</summary>
        public ushort RepeatNTimes { get; set; }
        /// <summary>Deprecated RGB tag color (3 bytes).</summary>
        public byte[] TagColor { get; set; } = new byte[3];
        public string TagName { get; set; }
    }
    public List<Tag> Tags { get; set; } = new();
}

/// <summary>Palette chunk (0x2019).</summary>
public class AsepritePaletteChunk : AsepriteChunk
{
    public class Entry
    {
        /// <summary>Entry flags: 1=Has name.</summary>
        public ushort Flags { get; set; }
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte A { get; set; }
        /// <summary>Color name (only if Flags bit 1 is set).</summary>
        public string Name { get; set; }
        public bool HasName => (Flags & 1) != 0;
    }
    public uint NewPaletteSize { get; set; }
    public uint FirstColorIndex { get; set; }
    public uint LastColorIndex { get; set; }
    public List<Entry> Entries { get; set; } = new();
}

/// <summary>User Data chunk (0x2020).</summary>
public class AsepriteUserDataChunk : AsepriteChunk
{
    public class Property
    {
        public string Name { get; set; }
        public ushort Type { get; set; }
        public object Value { get; set; }
    }
    public class PropertyMap
    {
        /// <summary>0=User properties, non-zero=Extension entry ID.</summary>
        public uint Key { get; set; }
        public List<Property> Properties { get; set; } = new();
    }
    /// <summary>Flags: 1=Has text, 2=Has color, 4=Has properties.</summary>
    public uint Flags { get; set; }
    public string Text { get; set; }
    /// <summary>RGBA color (4 bytes).</summary>
    public byte[] Color { get; set; } = new byte[4];
    /// <summary>Property maps (only if Flags bit 4 is set).</summary>
    public List<PropertyMap> PropertyMaps { get; set; } = new();

    public bool HasText => (Flags & 1) != 0;
    public bool HasColor => (Flags & 2) != 0;
    public bool HasProperties => (Flags & 4) != 0;
}

/// <summary>Slice chunk (0x2022).</summary>
public class AsepriteSliceChunk : AsepriteChunk
{
    public class SliceKey
    {
        public uint FrameNumber { get; set; }
        public int SliceX { get; set; }
        public int SliceY { get; set; }
        public uint SliceWidth { get; set; }
        public uint SliceHeight { get; set; }
        // 9-patch fields (Flags bit 1)
        public int CenterX { get; set; }
        public int CenterY { get; set; }
        public uint CenterWidth { get; set; }
        public uint CenterHeight { get; set; }
        // Pivot fields (Flags bit 2)
        public int PivotX { get; set; }
        public int PivotY { get; set; }
    }
    /// <summary>Flags: 1=9-patches slice, 2=Has pivot information.</summary>
    public uint Flags { get; set; }
    public string Name { get; set; }
    public List<SliceKey> SliceKeys { get; set; } = new();

    public bool IsNinePatch => (Flags & 1) != 0;
    public bool HasPivot => (Flags & 2) != 0;
}

/// <summary>Tileset chunk (0x2023).</summary>
public class AsepriteTilesetChunk : AsepriteChunk
{
    public uint TilesetId { get; set; }
    /// <summary>
    /// Flags: 1=Link to external file, 2=Include tiles inside file,
    /// 4=Tile ID=0 is empty, 8=Match X flip, 16=Match Y flip, 32=Match D flip.
    /// </summary>
    public uint Flags { get; set; }
    public uint NumberOfTiles { get; set; }
    public ushort TileWidth { get; set; }
    public ushort TileHeight { get; set; }
    /// <summary>Base index for display (default 1).</summary>
    public short BaseIndex { get; set; }
    public string Name { get; set; }
    // If Flags bit 1
    public uint ExternalFileId { get; set; }
    public uint TilesetIdInExternalFile { get; set; }
    // If Flags bit 2
    public byte[] CompressedTilesetImage { get; set; }

    public bool HasExternalFileLink => (Flags & 1) != 0;
    public bool HasTilesInside => (Flags & 2) != 0;
    public bool EmptyTileIsZero => (Flags & 4) != 0;
}

/// <summary>Catch-all for unknown/unrecognized chunk types.</summary>
public class AsepriteRawChunk : AsepriteChunk
{
    public byte[] Data { get; set; }
}
