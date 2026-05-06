using PixiEditor.Models.IO.CustomDocumentFormats.Aseprite;

namespace PixiEditor.Tests;

public class AsepriteTests
{
    #region Importer / Exporter Round-Trip

    [Fact]
    public void RoundTrip_SingleFrameRgba_PreservesHeaderAndPixelData()
    {
        var original = CreateMinimalRgbaFile(4, 4, pixelFill: 0xAA);

        var roundTripped = WriteAndRead(original);

        Assert.Equal(original.Width, roundTripped.Width);
        Assert.Equal(original.Height, roundTripped.Height);
        Assert.Equal(original.ColorDepth, roundTripped.ColorDepth);
        Assert.Equal(original.MagicNumber, roundTripped.MagicNumber);
        Assert.Equal((uint)0xA5E0, roundTripped.MagicNumber);
        Assert.Equal(original.Flags, roundTripped.Flags);
        Assert.Equal(original.TransparentIndex, roundTripped.TransparentIndex);
        Assert.Equal(original.PixelWidth, roundTripped.PixelWidth);
        Assert.Equal(original.PixelHeight, roundTripped.PixelHeight);

        Assert.Equal(original.Frames.Count, roundTripped.Frames.Count);
        Assert.Single(roundTripped.Frames);
    }

    [Fact]
    public void RoundTrip_MultipleFrames_PreservesFrameCount()
    {
        var original = CreateMinimalRgbaFile(2, 2, frameCount: 5);

        var roundTripped = WriteAndRead(original);

        Assert.Equal(5, roundTripped.Frames.Count);
    }

    [Fact]
    public void RoundTrip_FrameDuration_IsPreserved()
    {
        var original = CreateMinimalRgbaFile(2, 2, frameCount: 3);
        original.Frames[0].FrameDuration = 100;
        original.Frames[1].FrameDuration = 200;
        original.Frames[2].FrameDuration = 50;

        var roundTripped = WriteAndRead(original);

        Assert.Equal(100, roundTripped.Frames[0].FrameDuration);
        Assert.Equal(200, roundTripped.Frames[1].FrameDuration);
        Assert.Equal(50, roundTripped.Frames[2].FrameDuration);
    }

    [Fact]
    public void RoundTrip_LayerChunk_PreservesProperties()
    {
        var original = CreateMinimalRgbaFile(2, 2);
        var layer = original.Frames[0].Chunks.OfType<AsepriteLayerChunk>().First();
        layer.Name = "TestLayer";
        layer.Opacity = 128;
        layer.BlendMode = 3; // Overlay
        layer.Flags = 1; // Visible

        var roundTripped = WriteAndRead(original);

        var rtLayer = roundTripped.Frames[0].Chunks.OfType<AsepriteLayerChunk>().First();
        Assert.Equal("TestLayer", rtLayer.Name);
        Assert.Equal(128, rtLayer.Opacity);
        Assert.Equal(3, rtLayer.BlendMode);
        Assert.Equal(1, rtLayer.Flags);
    }

    [Fact]
    public void RoundTrip_MultipleLayers_PreservesOrder()
    {
        var original = CreateMinimalRgbaFile(2, 2);
        var frame = original.Frames[0];

        // Add a second layer
        frame.Chunks.Add(new AsepriteLayerChunk
        {
            ChunkType = 0x2004,
            Name = "Background",
            Opacity = 255,
            Flags = 1
        });

        // Add a third layer (group)
        frame.Chunks.Add(new AsepriteLayerChunk
        {
            ChunkType = 0x2004,
            Name = "Group1",
            LayerType = 1, // Group
            Flags = 1,
            ChildLevel = 0
        });

        var roundTripped = WriteAndRead(original);

        var layers = roundTripped.Frames[0].Chunks.OfType<AsepriteLayerChunk>().ToList();
        Assert.Equal(3, layers.Count);
        Assert.Equal("Layer 0", layers[0].Name);
        Assert.Equal("Background", layers[1].Name);
        Assert.Equal("Group1", layers[2].Name);
        Assert.True(layers[2].IsGroup);
    }

    [Fact]
    public void RoundTrip_CompressedCel_PreservesPixelData()
    {
        ushort w = 4, h = 4;
        byte[] rawPixels = CreateTestPixelData(w, h, 0xFF);

        var original = CreateMinimalRgbaFile(w, h);
        var cel = original.Frames[0].Chunks.OfType<AsepriteCelChunk>().First();
        Assert.Equal(2, cel.CelType); // Compressed
        Assert.NotNull(cel.CompressedPixelData);

        var roundTripped = WriteAndRead(original);
        var rtCel = roundTripped.Frames[0].Chunks.OfType<AsepriteCelChunk>().First();

        Assert.Equal(2, rtCel.CelType);
        Assert.Equal(w, rtCel.Width);
        Assert.Equal(h, rtCel.Height);

        // Decompress and verify
        byte[] decompressedOriginal = AsepriteExporter.DecompressPixelData(cel.CompressedPixelData);
        byte[] decompressedRT = AsepriteExporter.DecompressPixelData(rtCel.CompressedPixelData);

        Assert.Equal(decompressedOriginal, decompressedRT);
    }

    [Fact]
    public void RoundTrip_LinkedCel_PreservesFrameLink()
    {
        var original = CreateMinimalRgbaFile(2, 2, frameCount: 3);

        // Make frame 2's cel a linked cel pointing to frame 0
        var frame2 = original.Frames[2];
        frame2.Chunks.Clear();
        frame2.Chunks.Add(new AsepriteCelChunk
        {
            ChunkType = 0x2005,
            LayerIndex = 0,
            CelType = 1, // Linked
            LinkedFramePosition = 0,
            Opacity = 255
        });

        var roundTripped = WriteAndRead(original);

        var rtCel = roundTripped.Frames[2].Chunks.OfType<AsepriteCelChunk>().First();
        Assert.Equal(1, rtCel.CelType);
        Assert.Equal((ushort)0, rtCel.LinkedFramePosition);
    }

    [Fact]
    public void RoundTrip_PaletteChunk_PreservesColors()
    {
        var original = CreateMinimalRgbaFile(2, 2);
        var frame = original.Frames[0];

        var palette = new AsepritePaletteChunk
        {
            ChunkType = 0x2019,
            NewPaletteSize = 3,
            FirstColorIndex = 0,
            LastColorIndex = 2,
            Entries = new List<AsepritePaletteChunk.Entry>
            {
                new() { R = 255, G = 0, B = 0, A = 255, Flags = 0 },
                new() { R = 0, G = 255, B = 0, A = 255, Flags = 0 },
                new() { R = 0, G = 0, B = 255, A = 128, Flags = 0 },
            }
        };
        frame.Chunks.Add(palette);

        var roundTripped = WriteAndRead(original);

        var rtPalette = roundTripped.Frames[0].Chunks.OfType<AsepritePaletteChunk>().First();
        Assert.Equal(3, rtPalette.Entries.Count);
        Assert.Equal(255, rtPalette.Entries[0].R);
        Assert.Equal(255, rtPalette.Entries[1].G);
        Assert.Equal(255, rtPalette.Entries[2].B);
        Assert.Equal(128, rtPalette.Entries[2].A);
    }

    [Fact]
    public void RoundTrip_TagsChunk_PreservesTags()
    {
        var original = CreateMinimalRgbaFile(2, 2, frameCount: 10);
        var frame = original.Frames[0];

        var tags = new AsepriteTagsChunk
        {
            ChunkType = 0x2018,
            Tags = new List<AsepriteTagsChunk.Tag>
            {
                new()
                {
                    FromFrame = 0, ToFrame = 4,
                    LoopAnimationDirection = 0,
                    RepeatNTimes = 0,
                    TagColor = new byte[] { 255, 0, 0 },
                    TagName = "Idle"
                },
                new()
                {
                    FromFrame = 5, ToFrame = 9,
                    LoopAnimationDirection = 1,
                    RepeatNTimes = 3,
                    TagColor = new byte[] { 0, 255, 0 },
                    TagName = "Walk"
                }
            }
        };
        frame.Chunks.Add(tags);

        var roundTripped = WriteAndRead(original);

        var rtTags = roundTripped.Frames[0].Chunks.OfType<AsepriteTagsChunk>().First();
        Assert.Equal(2, rtTags.Tags.Count);
        Assert.Equal("Idle", rtTags.Tags[0].TagName);
        Assert.Equal((ushort)0, rtTags.Tags[0].FromFrame);
        Assert.Equal((ushort)4, rtTags.Tags[0].ToFrame);
        Assert.Equal("Walk", rtTags.Tags[1].TagName);
        Assert.Equal((byte)1, rtTags.Tags[1].LoopAnimationDirection);
        Assert.Equal((ushort)3, rtTags.Tags[1].RepeatNTimes);
    }

    [Fact]
    public void RoundTrip_ColorProfileChunk_sRGB_Preserved()
    {
        var original = CreateMinimalRgbaFile(2, 2);
        original.Frames[0].Chunks.Add(new AsepriteColorProfileChunk
        {
            ChunkType = 0x2007,
            Type = 1, // sRGB
            Flags = 0,
            FixedGamma = 0
        });

        var roundTripped = WriteAndRead(original);

        var rtProfile = roundTripped.Frames[0].Chunks.OfType<AsepriteColorProfileChunk>().First();
        Assert.Equal(1, rtProfile.Type);
    }

    [Fact]
    public void RoundTrip_SliceChunk_PreservesData()
    {
        var original = CreateMinimalRgbaFile(16, 16);
        original.Frames[0].Chunks.Add(new AsepriteSliceChunk
        {
            ChunkType = 0x2022,
            Flags = 0,
            Name = "TestSlice",
            SliceKeys = new List<AsepriteSliceChunk.SliceKey>
            {
                new()
                {
                    FrameNumber = 0,
                    SliceX = 2, SliceY = 2,
                    SliceWidth = 12, SliceHeight = 12
                }
            }
        });

        var roundTripped = WriteAndRead(original);

        var rtSlice = roundTripped.Frames[0].Chunks.OfType<AsepriteSliceChunk>().First();
        Assert.Equal("TestSlice", rtSlice.Name);
        Assert.Single(rtSlice.SliceKeys);
        Assert.Equal(2, rtSlice.SliceKeys[0].SliceX);
        Assert.Equal(12u, rtSlice.SliceKeys[0].SliceWidth);
    }

    #endregion

    #region Compression

    [Fact]
    public void CompressDecompress_RoundTrips()
    {
        byte[] original = new byte[256];
        for (int i = 0; i < original.Length; i++)
            original[i] = (byte)(i % 64);

        byte[] compressed = AsepriteExporter.CompressPixelData(original);
        byte[] decompressed = AsepriteExporter.DecompressPixelData(compressed);

        Assert.Equal(original, decompressed);
    }

    [Fact]
    public void CompressDecompress_EmptyData_ReturnsEmpty()
    {
        byte[] compressed = AsepriteExporter.CompressPixelData(Array.Empty<byte>());
        byte[] decompressed = AsepriteExporter.DecompressPixelData(compressed);

        Assert.Empty(compressed);
        Assert.Empty(decompressed);
    }

    [Fact]
    public void CompressDecompress_AllZeros()
    {
        byte[] original = new byte[1024];

        byte[] compressed = AsepriteExporter.CompressPixelData(original);
        byte[] decompressed = AsepriteExporter.DecompressPixelData(compressed);

        Assert.Equal(original, decompressed);
        // Compressed should be much smaller than original (all zeros compresses well)
        Assert.True(compressed.Length < original.Length);
    }

    [Fact]
    public void CompressDecompress_LargeRandomData()
    {
        var rng = new Random(42);
        byte[] original = new byte[4096];
        rng.NextBytes(original);

        byte[] compressed = AsepriteExporter.CompressPixelData(original);
        byte[] decompressed = AsepriteExporter.DecompressPixelData(compressed);

        Assert.Equal(original, decompressed);
    }

    #endregion

    #region Importer Validation

    [Fact]
    public void Import_InvalidMagicNumber_Throws()
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        // Write a fake header with wrong magic number
        writer.Write((uint)128); // File size
        writer.Write((ushort)0xDEAD); // Wrong magic
        writer.Write(new byte[122]); // Rest of header

        ms.Position = 0;
        Assert.Throws<InvalidDataException>(() => AsepriteImporter.Read(ms));
    }

    [Fact]
    public void Import_ZeroFrames_ReturnsEmptyFramesList()
    {
        // Build a valid header but with 0 frames
        var file = new AsepriteFile
        {
            Width = 2,
            Height = 2,
            ColorDepth = 32,
            Flags = 1
        };
        // No frames added

        var roundTripped = WriteAndRead(file);

        Assert.Empty(roundTripped.Frames);
    }

    #endregion

    #region CreateFromLayers

    [Fact]
    public void CreateFromLayers_SingleLayer_SingleFrame()
    {
        ushort w = 8, h = 8;
        byte[] pixels = CreateTestPixelData(w, h, 0xCC);

        var layers = new List<AsepriteLayerInfo>
        {
            new() { Name = "MyLayer", Opacity = 200, BlendMode = 0 }
        };

        var frames = new List<AsepriteFrameInfo>
        {
            new()
            {
                DurationMs = 150,
                Cels = new List<AsepriteCelInfo>
                {
                    new()
                    {
                        LayerIndex = 0, X = 0, Y = 0,
                        Opacity = 255, Width = w, Height = h,
                        PixelData = pixels
                    }
                }
            }
        };

        var file = AsepriteExporter.CreateFromLayers(w, h, layers, frames);

        Assert.Equal(w, file.Width);
        Assert.Equal(h, file.Height);
        Assert.Equal(32, file.ColorDepth);
        Assert.Single(file.Frames);

        // First frame should have layer chunk + cel chunk
        var layerChunks = file.Frames[0].Chunks.OfType<AsepriteLayerChunk>().ToList();
        var celChunks = file.Frames[0].Chunks.OfType<AsepriteCelChunk>().ToList();

        Assert.Single(layerChunks);
        Assert.Single(celChunks);
        Assert.Equal("MyLayer", layerChunks[0].Name);
        Assert.Equal(200, layerChunks[0].Opacity);
        Assert.Equal(150, file.Frames[0].FrameDuration);

        // Verify cel pixel data round-trips
        byte[] decompressed = AsepriteExporter.DecompressPixelData(celChunks[0].CompressedPixelData);
        Assert.Equal(pixels, decompressed);
    }

    [Fact]
    public void CreateFromLayers_MultipleFrames_LayerChunksOnlyOnFirstFrame()
    {
        var layers = new List<AsepriteLayerInfo>
        {
            new() { Name = "Layer1" },
            new() { Name = "Layer2" }
        };

        byte[] pixels = CreateTestPixelData(2, 2, 0xFF);

        var frames = new List<AsepriteFrameInfo>
        {
            new()
            {
                DurationMs = 100,
                Cels = new List<AsepriteCelInfo>
                {
                    new() { LayerIndex = 0, Width = 2, Height = 2, PixelData = pixels, Opacity = 255 }
                }
            },
            new()
            {
                DurationMs = 100,
                Cels = new List<AsepriteCelInfo>
                {
                    new() { LayerIndex = 0, Width = 2, Height = 2, PixelData = pixels, Opacity = 255 }
                }
            },
            new()
            {
                DurationMs = 100,
                Cels = new List<AsepriteCelInfo>
                {
                    new() { LayerIndex = 0, Width = 2, Height = 2, PixelData = pixels, Opacity = 255 }
                }
            }
        };

        var file = AsepriteExporter.CreateFromLayers(2, 2, layers, frames);

        // Layer chunks only on frame 0
        Assert.Equal(2, file.Frames[0].Chunks.OfType<AsepriteLayerChunk>().Count());
        Assert.Empty(file.Frames[1].Chunks.OfType<AsepriteLayerChunk>());
        Assert.Empty(file.Frames[2].Chunks.OfType<AsepriteLayerChunk>());
    }

    [Fact]
    public void CreateFromLayers_LinkedCel_IsPreserved()
    {
        var layers = new List<AsepriteLayerInfo> { new() { Name = "Base" } };
        byte[] pixels = CreateTestPixelData(2, 2, 0xFF);

        var frames = new List<AsepriteFrameInfo>
        {
            new()
            {
                DurationMs = 100,
                Cels = new List<AsepriteCelInfo>
                {
                    new() { LayerIndex = 0, Width = 2, Height = 2, PixelData = pixels, Opacity = 255 }
                }
            },
            new()
            {
                DurationMs = 100,
                Cels = new List<AsepriteCelInfo>
                {
                    new() { LayerIndex = 0, IsLinked = true, LinkedFrame = 0, Opacity = 255 }
                }
            }
        };

        var file = AsepriteExporter.CreateFromLayers(2, 2, layers, frames);

        var cel0 = file.Frames[0].Chunks.OfType<AsepriteCelChunk>().First();
        var cel1 = file.Frames[1].Chunks.OfType<AsepriteCelChunk>().First();

        Assert.Equal(2, cel0.CelType); // Compressed
        Assert.Equal(1, cel1.CelType); // Linked
        Assert.Equal(0, cel1.LinkedFramePosition);
    }

    #endregion

    #region FramesCount property

    [Fact]
    public void FramesCount_ReflectsActualFrameCount()
    {
        var file = new AsepriteFile { Width = 2, Height = 2, ColorDepth = 32 };
        Assert.Equal((ushort)0, file.FramesCount);

        file.Frames.Add(new AsepriteFrame());
        Assert.Equal((ushort)1, file.FramesCount);

        file.Frames.Add(new AsepriteFrame());
        file.Frames.Add(new AsepriteFrame());
        Assert.Equal((ushort)3, file.FramesCount);
    }

    #endregion

    #region Layer properties

    [Fact]
    public void LayerChunk_IsGroup_CorrectlyReflectsLayerType()
    {
        var imageLayer = new AsepriteLayerChunk { LayerType = 0 };
        var groupLayer = new AsepriteLayerChunk { LayerType = 1 };
        var tilemapLayer = new AsepriteLayerChunk { LayerType = 2 };

        Assert.False(imageLayer.IsGroup);
        Assert.True(groupLayer.IsGroup);
        Assert.False(tilemapLayer.IsGroup);

        Assert.False(imageLayer.IsTilemap);
        Assert.False(groupLayer.IsTilemap);
        Assert.True(tilemapLayer.IsTilemap);
    }

    [Fact]
    public void LayerChunk_IsVisible_ReflectsFlags()
    {
        var visible = new AsepriteLayerChunk { Flags = 1 };
        var hidden = new AsepriteLayerChunk { Flags = 0 };
        var visibleWithOtherFlags = new AsepriteLayerChunk { Flags = 0b11 };

        Assert.True(visible.IsVisible);
        Assert.False(hidden.IsVisible);
        Assert.True(visibleWithOtherFlags.IsVisible);
    }

    #endregion

    #region File flags

    [Fact]
    public void AsepriteFile_LayerOpacityValid_ReflectsFlag()
    {
        var file = new AsepriteFile { Flags = 0 };
        Assert.False(file.LayerOpacityValid);

        file.Flags = 1;
        Assert.True(file.LayerOpacityValid);

        file.Flags = 0b10; // other flag, not opacity
        Assert.False(file.LayerOpacityValid);
    }

    [Fact]
    public void AsepriteFile_LayersHaveUuid_ReflectsFlag()
    {
        var file = new AsepriteFile { Flags = 0 };
        Assert.False(file.LayersHaveUuid);

        file.Flags = 0b100; // bit 2
        Assert.True(file.LayersHaveUuid);
    }

    #endregion

    #region Grid properties

    [Fact]
    public void RoundTrip_GridProperties_Preserved()
    {
        var original = CreateMinimalRgbaFile(16, 16);
        original.GridX = 4;
        original.GridY = 4;
        original.GridWidth = 8;
        original.GridHeight = 8;

        var roundTripped = WriteAndRead(original);

        Assert.Equal(4, roundTripped.GridX);
        Assert.Equal(4, roundTripped.GridY);
        Assert.Equal((ushort)8, roundTripped.GridWidth);
        Assert.Equal((ushort)8, roundTripped.GridHeight);
    }

    #endregion

    #region Edge cases

    [Fact]
    public void RoundTrip_1x1Image_Works()
    {
        var original = CreateMinimalRgbaFile(1, 1);

        var roundTripped = WriteAndRead(original);

        Assert.Equal(1, roundTripped.Width);
        Assert.Equal(1, roundTripped.Height);
        Assert.Single(roundTripped.Frames);
    }

    [Fact]
    public void RoundTrip_LargeImage_Works()
    {
        // 256x256 - large enough to exercise multi-chunk compression
        var original = CreateMinimalRgbaFile(256, 256);

        var roundTripped = WriteAndRead(original);

        Assert.Equal(256, roundTripped.Width);
        Assert.Equal(256, roundTripped.Height);

        var cel = roundTripped.Frames[0].Chunks.OfType<AsepriteCelChunk>().First();
        byte[] decompressed = AsepriteExporter.DecompressPixelData(cel.CompressedPixelData);
        Assert.Equal(256 * 256 * 4, decompressed.Length);
    }

    [Fact]
    public void RoundTrip_UserDataChunk_PreservesTextAndColor()
    {
        var original = CreateMinimalRgbaFile(2, 2);
        original.Frames[0].Chunks.Add(new AsepriteUserDataChunk
        {
            ChunkType = 0x2020,
            Flags = 3, // HasText | HasColor
            Text = "hello world",
            Color = new byte[] { 255, 128, 64, 200 }
        });

        var roundTripped = WriteAndRead(original);

        var rtUserData = roundTripped.Frames[0].Chunks.OfType<AsepriteUserDataChunk>().First();
        Assert.True(rtUserData.HasText);
        Assert.True(rtUserData.HasColor);
        Assert.Equal("hello world", rtUserData.Text);
        Assert.Equal(new byte[] { 255, 128, 64, 200 }, rtUserData.Color);
    }

    [Fact]
    public void Import_FromFilePath_Works()
    {
        var original = CreateMinimalRgbaFile(4, 4);
        string tempPath = Path.Combine(Path.GetTempPath(), $"aseprite_test_{Guid.NewGuid()}.ase");

        try
        {
            AsepriteExporter.Write(tempPath, original);
            var imported = AsepriteImporter.Read(tempPath);

            Assert.Equal(4, imported.Width);
            Assert.Equal(4, imported.Height);
            Assert.Single(imported.Frames);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    #endregion

    #region Indexed Color Depth (8-bit)

    [Fact]
    public void RoundTrip_IndexedColorDepth_PreservesSingleBytePixelData()
    {
        ushort w = 4, h = 4;

        // 8-bit indexed: 1 byte per pixel (palette index)
        byte[] indexedPixels = new byte[w * h];
        for (int i = 0; i < indexedPixels.Length; i++)
            indexedPixels[i] = (byte)(i % 4); // indices 0,1,2,3 repeating

        var file = new AsepriteFile
        {
            Width = w,
            Height = h,
            ColorDepth = 8, // Indexed
            Flags = 1,
            TransparentIndex = 0,
            PixelWidth = 1,
            PixelHeight = 1,
            GridWidth = 16,
            GridHeight = 16
        };

        var frame = new AsepriteFrame { FrameDuration = 100 };
        frame.Chunks.Add(new AsepriteLayerChunk
        {
            ChunkType = 0x2004,
            Name = "Indexed Layer",
            Flags = 1,
            Opacity = 255
        });

        frame.Chunks.Add(new AsepriteCelChunk
        {
            ChunkType = 0x2005,
            LayerIndex = 0,
            X = 0, Y = 0,
            Opacity = 255,
            CelType = 2, // Compressed
            Width = w,
            Height = h,
            CompressedPixelData = AsepriteExporter.CompressPixelData(indexedPixels)
        });

        // Add a palette so the file is realistic
        frame.Chunks.Add(new AsepritePaletteChunk
        {
            ChunkType = 0x2019,
            NewPaletteSize = 4,
            FirstColorIndex = 0,
            LastColorIndex = 3,
            Entries = new List<AsepritePaletteChunk.Entry>
            {
                new() { R = 0,   G = 0,   B = 0,   A = 0,   Flags = 0 }, // transparent
                new() { R = 255, G = 0,   B = 0,   A = 255, Flags = 0 },
                new() { R = 0,   G = 255, B = 0,   A = 255, Flags = 0 },
                new() { R = 0,   G = 0,   B = 255, A = 255, Flags = 0 },
            }
        });

        file.Frames.Add(frame);

        var roundTripped = WriteAndRead(file);

        Assert.Equal((ushort)8, roundTripped.ColorDepth);
        Assert.Single(roundTripped.Frames);

        var rtCel = roundTripped.Frames[0].Chunks.OfType<AsepriteCelChunk>().First();
        Assert.Equal(2, rtCel.CelType);
        Assert.Equal(w, rtCel.Width);
        Assert.Equal(h, rtCel.Height);

        // Decompressed pixel data should be 1 byte per pixel (w*h), not 4
        byte[] decompressed = AsepriteExporter.DecompressPixelData(rtCel.CompressedPixelData);
        Assert.Equal(w * h, decompressed.Length); // 16 bytes, not 64
        Assert.Equal(indexedPixels, decompressed);
    }

    [Fact]
    public void RoundTrip_IndexedColorDepth_TransparentIndexPreserved()
    {
        var file = new AsepriteFile
        {
            Width = 2,
            Height = 2,
            ColorDepth = 8,
            Flags = 1,
            TransparentIndex = 5,
            PixelWidth = 1,
            PixelHeight = 1
        };

        byte[] indexedPixels = { 5, 1, 2, 5 }; // indices 5 are transparent

        var frame = new AsepriteFrame { FrameDuration = 100 };
        frame.Chunks.Add(new AsepriteLayerChunk
        {
            ChunkType = 0x2004,
            Name = "Layer",
            Flags = 1,
            Opacity = 255
        });
        frame.Chunks.Add(new AsepriteCelChunk
        {
            ChunkType = 0x2005,
            LayerIndex = 0,
            CelType = 2,
            Width = 2,
            Height = 2,
            Opacity = 255,
            CompressedPixelData = AsepriteExporter.CompressPixelData(indexedPixels)
        });
        file.Frames.Add(frame);

        var roundTripped = WriteAndRead(file);

        Assert.Equal((byte)5, roundTripped.TransparentIndex);

        byte[] decompressed = AsepriteExporter.DecompressPixelData(
            roundTripped.Frames[0].Chunks.OfType<AsepriteCelChunk>().First().CompressedPixelData);
        Assert.Equal(indexedPixels, decompressed);
    }

    #endregion

    #region UserData Chunk Ordering

    [Fact]
    public void RoundTrip_UserDataAfterCel_PreservesChunkOrder()
    {
        // Per spec: User Data chunk (0x2020) applies to the *previous* chunk.
        // This test verifies the chunk ordering is preserved through round-trip,
        // which is the most common cause of "shifted" data bugs in parsers.
        var original = CreateMinimalRgbaFile(2, 2);
        var frame = original.Frames[0];

        // The frame already has: [LayerChunk, CelChunk]
        // Add a UserData chunk after the CelChunk — it should apply to the cel
        frame.Chunks.Add(new AsepriteUserDataChunk
        {
            ChunkType = 0x2020,
            Flags = 1, // HasText
            Text = "cel annotation"
        });

        var roundTripped = WriteAndRead(original);

        var chunks = roundTripped.Frames[0].Chunks;

        // Verify chunk ordering is preserved: Layer, Cel, UserData
        Assert.Equal(3, chunks.Count);
        Assert.IsType<AsepriteLayerChunk>(chunks[0]);
        Assert.IsType<AsepriteCelChunk>(chunks[1]);
        Assert.IsType<AsepriteUserDataChunk>(chunks[2]);

        var rtUserData = (AsepriteUserDataChunk)chunks[2];
        Assert.Equal("cel annotation", rtUserData.Text);
    }

    [Fact]
    public void RoundTrip_UserDataAfterLayer_PreservesChunkOrder()
    {
        var file = new AsepriteFile
        {
            Width = 2, Height = 2,
            ColorDepth = 32, Flags = 1,
            PixelWidth = 1, PixelHeight = 1,
            GridWidth = 16, GridHeight = 16
        };

        var frame = new AsepriteFrame { FrameDuration = 100 };

        // Layer chunk
        frame.Chunks.Add(new AsepriteLayerChunk
        {
            ChunkType = 0x2004,
            Name = "Annotated Layer",
            Flags = 1,
            Opacity = 255
        });

        // UserData immediately after Layer — applies to that layer
        frame.Chunks.Add(new AsepriteUserDataChunk
        {
            ChunkType = 0x2020,
            Flags = 3, // HasText | HasColor
            Text = "layer notes",
            Color = new byte[] { 100, 150, 200, 255 }
        });

        // Cel chunk (separate from the layer's user data)
        byte[] raw = CreateTestPixelData(2, 2, 0xAA);
        frame.Chunks.Add(new AsepriteCelChunk
        {
            ChunkType = 0x2005,
            LayerIndex = 0,
            CelType = 2,
            Width = 2, Height = 2,
            Opacity = 255,
            CompressedPixelData = AsepriteExporter.CompressPixelData(raw)
        });

        file.Frames.Add(frame);

        var roundTripped = WriteAndRead(file);

        var chunks = roundTripped.Frames[0].Chunks;

        // Verify: Layer, UserData(for layer), Cel
        Assert.Equal(3, chunks.Count);
        Assert.IsType<AsepriteLayerChunk>(chunks[0]);
        Assert.IsType<AsepriteUserDataChunk>(chunks[1]);
        Assert.IsType<AsepriteCelChunk>(chunks[2]);

        var rtUserData = (AsepriteUserDataChunk)chunks[1];
        Assert.Equal("layer notes", rtUserData.Text);
        Assert.True(rtUserData.HasColor);
        Assert.Equal(100, rtUserData.Color[0]);
    }

    [Fact]
    public void RoundTrip_MultipleUserDataChunks_EachFollowsCorrectParent()
    {
        var file = new AsepriteFile
        {
            Width = 2, Height = 2,
            ColorDepth = 32, Flags = 1,
            PixelWidth = 1, PixelHeight = 1,
            GridWidth = 16, GridHeight = 16
        };

        var frame = new AsepriteFrame { FrameDuration = 100 };
        byte[] raw = CreateTestPixelData(2, 2, 0xFF);

        // Layer + its UserData
        frame.Chunks.Add(new AsepriteLayerChunk
        {
            ChunkType = 0x2004, Name = "Layer A", Flags = 1, Opacity = 255
        });
        frame.Chunks.Add(new AsepriteUserDataChunk
        {
            ChunkType = 0x2020, Flags = 1, Text = "for layer"
        });

        // Cel + its UserData
        frame.Chunks.Add(new AsepriteCelChunk
        {
            ChunkType = 0x2005, LayerIndex = 0, CelType = 2,
            Width = 2, Height = 2, Opacity = 255,
            CompressedPixelData = AsepriteExporter.CompressPixelData(raw)
        });
        frame.Chunks.Add(new AsepriteUserDataChunk
        {
            ChunkType = 0x2020, Flags = 1, Text = "for cel"
        });

        file.Frames.Add(frame);

        var roundTripped = WriteAndRead(file);

        var chunks = roundTripped.Frames[0].Chunks;
        Assert.Equal(4, chunks.Count);

        // Order: Layer, UserData("for layer"), Cel, UserData("for cel")
        Assert.IsType<AsepriteLayerChunk>(chunks[0]);
        Assert.IsType<AsepriteUserDataChunk>(chunks[1]);
        Assert.IsType<AsepriteCelChunk>(chunks[2]);
        Assert.IsType<AsepriteUserDataChunk>(chunks[3]);

        Assert.Equal("for layer", ((AsepriteUserDataChunk)chunks[1]).Text);
        Assert.Equal("for cel", ((AsepriteUserDataChunk)chunks[3]).Text);
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Creates a minimal valid AsepriteFile with one layer and the specified number of frames,
    /// each with a compressed RGBA cel filled with a solid byte value.
    /// </summary>
    private static AsepriteFile CreateMinimalRgbaFile(
        ushort width, ushort height,
        byte pixelFill = 0xFF,
        int frameCount = 1)
    {
        var file = new AsepriteFile
        {
            Width = width,
            Height = height,
            ColorDepth = 32,
            Flags = 1,
            PixelWidth = 1,
            PixelHeight = 1,
            GridWidth = 16,
            GridHeight = 16
        };

        for (int f = 0; f < frameCount; f++)
        {
            var frame = new AsepriteFrame { FrameDuration = 100 };

            // Add layer chunk only on first frame (per spec)
            if (f == 0)
            {
                frame.Chunks.Add(new AsepriteLayerChunk
                {
                    ChunkType = 0x2004,
                    Name = "Layer 0",
                    Flags = 1,
                    Opacity = 255,
                    BlendMode = 0,
                    LayerType = 0
                });
            }

            // Add compressed cel
            byte[] raw = CreateTestPixelData(width, height, pixelFill);
            frame.Chunks.Add(new AsepriteCelChunk
            {
                ChunkType = 0x2005,
                LayerIndex = 0,
                X = 0,
                Y = 0,
                Opacity = 255,
                CelType = 2, // Compressed
                Width = width,
                Height = height,
                CompressedPixelData = AsepriteExporter.CompressPixelData(raw)
            });

            file.Frames.Add(frame);
        }

        return file;
    }

    /// <summary>
    /// Creates RGBA pixel data filled with a repeating byte pattern.
    /// </summary>
    private static byte[] CreateTestPixelData(int width, int height, byte fill)
    {
        byte[] data = new byte[width * height * 4];
        for (int i = 0; i < data.Length; i += 4)
        {
            data[i] = fill;       // R
            data[i + 1] = fill;   // G
            data[i + 2] = fill;   // B
            data[i + 3] = 255;    // A
        }
        return data;
    }

    /// <summary>
    /// Serializes then deserializes an AsepriteFile through an in-memory stream.
    /// </summary>
    private static AsepriteFile WriteAndRead(AsepriteFile file)
    {
        using var ms = new MemoryStream();
        AsepriteExporter.Write(ms, file);
        ms.Position = 0;
        return AsepriteImporter.Read(ms);
    }

    #endregion
}
