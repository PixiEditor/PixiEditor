using System.Diagnostics;
using ChunkyImageLib;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Helpers;
using PixiEditor.Models.IO.CustomDocumentFormats.Aseprite;
using PixiEditor.Parser;
using PixiEditor.Parser.Graph;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Models.IO.CustomDocumentFormats;

internal class AsepriteDocumentBuilder : IDocumentBuilder
{
    public IReadOnlyCollection<string> Extensions => new List<string>() { ".ase", ".aseprite" };

    public void Build(DocumentViewModelBuilder builder, string path)
    {
        AsepriteFile aseFile = AsepriteImporter.Read(path);

        builder.WithSize(aseFile.Width, aseFile.Height);

        // Collect layers, palette from all frames
        var layers = new List<AsepriteLayerChunk>();
        AsepritePaletteChunk paletteChunk = null;
        AsepriteOldPaletteChunk0004 oldPaletteChunk0004 = null;
        bool hasNewPalette = false;

        foreach (var frame in aseFile.Frames)
        {
            foreach (var chunk in frame.Chunks)
            {
                switch (chunk)
                {
                    case AsepriteLayerChunk layerChunk:
                        layers.Add(layerChunk);
                        break;
                    case AsepritePaletteChunk pal:
                        paletteChunk = pal;
                        hasNewPalette = true;
                        break;
                    case AsepriteOldPaletteChunk0004 oldPal when !hasNewPalette:
                        oldPaletteChunk0004 = oldPal;
                        break;
                    // TODO: implement tags
                }
            }
        }

        // Build palette lookup
        byte[][] palette = BuildPalette(paletteChunk, oldPaletteChunk0004, aseFile.ColorCount);
        SetSwatches(builder, palette);

        bool hasAnimation = aseFile.Frames.Count > 1;
        
        if (layers.Count == 0)
        {
            BuildSingleLayerDocument(builder, aseFile, palette);
            return;
        }

        // Filter to only image layers
        var imageLayers = new List<(int index, AsepriteLayerChunk layer)>();
        for (int i = 0; i < layers.Count; i++)
        {
            if (!layers[i].IsGroup && !layers[i].IsTilemap)
                imageLayers.Add((i, layers[i]));
        }

        if (imageLayers.Count == 0)
        {
            BuildSingleLayerDocument(builder, aseFile, palette);
            return;
        }

        BuildLayeredDocument(builder, aseFile, imageLayers, palette, hasAnimation);
    }

    private void BuildSingleLayerDocument(DocumentViewModelBuilder builder, AsepriteFile aseFile, byte[][] palette)
    {
        Surface surface = RenderFlatFrame(aseFile, 0, palette);
        builder.WithGraph(graph =>
        {
            graph.WithImageLayerNode(
                new LocalizedString("IMAGE"),
                surface,
                ColorSpace.CreateSrgb(),
                out int id);
            graph.WithOutputNode(id, RenderNode.OutputPropertyName);
        });
    }

    private void BuildLayeredDocument(
        DocumentViewModelBuilder builder,
        AsepriteFile aseFile,
        List<(int index, AsepriteLayerChunk layer)> imageLayers,
        byte[][] palette,
        bool hasAnimation)
    {
        // Build graph with all image layers connected in a chain
        builder.WithGraph(graph =>
        {
            int? previousNodeId = null;
            Vector2 currentPosition = new Vector2();
            for (int i = 0; i < imageLayers.Count; i++)
            {
                var (layerIndex, layer) = imageLayers[i];

                // Render first frame for this layer
                Surface layerSurface = RenderLayerCel(aseFile, 0, layerIndex, palette);
                string layerName = layer.Name ?? $"Layer {i}";

                graph.WithImageLayerNode(
                    layerName,
                    layerSurface,
                    ColorSpace.CreateSrgb(),
                    out int nodeId);

                var nodeBuilder = graph.AllNodes[^1];
                nodeBuilder.WithInputValues(new Dictionary<string, object>()
                {
                    { StructureNode.OpacityPropertyName, aseFile.LayerOpacityValid ? layer.Opacity / 255f : 1f },
                    { StructureNode.IsVisiblePropertyName, layer.IsVisible },
                    { StructureNode.BlendModePropertyName, MapBlendMode(layer.BlendMode) }
                });
                nodeBuilder.WithPosition(currentPosition);
                currentPosition.X += 250;
                // Connect to previous layer's output
                if (previousNodeId != null)
                {
                    nodeBuilder.WithConnections(new[]
                    {
                        new PropertyConnection
                        {
                            InputPropertyName = "Background",
                            OutputPropertyName = RenderNode.OutputPropertyName,
                            OutputNodeId = previousNodeId.Value
                        }
                    });
                }

                previousNodeId = nodeId;
            }

            // Connect output node to the last layer
            if (previousNodeId != null)
            {
                graph.WithOutputNode(previousNodeId.Value, RenderNode.OutputPropertyName);
                graph.AllNodes[^1].Position = currentPosition;
            }
        });

        // Set up animation if multi-frame
        if (hasAnimation)
        {
            SetupAnimationData(builder, aseFile, imageLayers, palette);
        }
    }

    private void SetupAnimationData(
        DocumentViewModelBuilder builder,
        AsepriteFile aseFile,
        List<(int index, AsepriteLayerChunk layer)> imageLayers,
        byte[][] palette)
    {
        AnimationData animationData = new AnimationData();
        animationData.KeyFrameGroups = new List<KeyFrameGroup>();

        // Calculate FPS from average frame duration
        if (aseFile.Frames.Count > 0)
        {
            double avgDuration = aseFile.Frames.Average(f => f.FrameDuration > 0 ? f.FrameDuration : 100);
            animationData.FrameRate = Math.Max(1, (int)Math.Round(1000.0 / avgDuration));
        }

        var imageLayerNodes = builder.Graph?.AllNodes
            .Where(n => n.UniqueNodeName == "PixiEditor.ImageLayer")
            .ToList();
        
        if (imageLayerNodes == null || imageLayerNodes.Count == 0)
            return;
        
        for (int i = 0; i < imageLayers.Count && i < imageLayerNodes.Count; i++)
        {
            var (layerIndex, _) = imageLayers[i];
            var layerNode = imageLayerNodes[i];

            KeyFrameGroup layerGroup = new KeyFrameGroup { Enabled = true };
            layerGroup.NodeId = layerNode.Id;

            List<KeyFrameData> keyFrames = new List<KeyFrameData>();
            if (layerNode.KeyFrames != null)
                keyFrames.AddRange(layerNode.KeyFrames);

            int currentFrame = 1;
            for (int f = 0; f < aseFile.Frames.Count; f++)
            {
                Surface celSurface = RenderLayerCel(aseFile, f, layerIndex, palette);

                int durationMs = aseFile.Frames[f].FrameDuration > 0 ? aseFile.Frames[f].FrameDuration : 100;
                int durationTicks = Math.Max(1,
                    (int)Math.Round(durationMs / (1000.0 / animationData.FrameRate)));

                KeyFrameData keyFrameData = new KeyFrameData
                {
                    Id = keyFrames.Count,
                    AffectedElement = ImageLayerNode.ImageLayerKey,
                    Data = new ChunkyImage(celSurface),
                    StartFrame = currentFrame,
                    Duration = durationTicks,
                    IsVisible = true
                };

                keyFrames.Add(keyFrameData);
                layerGroup.Children.Add(new ElementKeyFrame
                {
                    NodeId = layerGroup.NodeId,
                    KeyFrameId = keyFrameData.Id
                });

                currentFrame += durationTicks;
            }

            layerNode.KeyFrames = keyFrames.ToArray();
            animationData.KeyFrameGroups.Add(layerGroup);
        }

        animationData.DefaultEndFrame = Math.Max(1, imageLayers.Count > 0 ? 
            aseFile.Frames.Sum(f => Math.Max(1, (int)Math.Round((f.FrameDuration > 0 ? f.FrameDuration : 100) / (1000.0 / animationData.FrameRate)))) : 1);
        
        builder.WithAnimationData(animationData, null);
    }

    /// <summary>
    /// Renders a specific layer's cel for the given frame to a Surface.
    /// </summary>
    private Surface RenderLayerCel(AsepriteFile aseFile, int frameIndex, int layerIndex, byte[][] palette)
    {
        var surface = new Surface(new VecI(aseFile.Width, aseFile.Height));

        if (frameIndex >= aseFile.Frames.Count)
            return surface;

        var frame = aseFile.Frames[frameIndex];

        // Find the cel for this layer in this frame
        AsepriteCelChunk celChunk = null;
        foreach (var chunk in frame.Chunks)
        {
            if (chunk is AsepriteCelChunk cel && cel.LayerIndex == layerIndex)
            {
                celChunk = cel;
                break;
            }
        }

        if (celChunk == null)
            return surface;

        // Handle linked cels - follow the link
        if (celChunk.CelType == 1)
        {
            return RenderLayerCel(aseFile, celChunk.LinkedFramePosition, layerIndex, palette);
        }

        byte[] pixelData = GetDecompressedPixelData(celChunk);
        if (pixelData == null || pixelData.Length == 0)
            return surface;

        byte[] rgba32 = ConvertToRgba32(pixelData, celChunk.Width, celChunk.Height,
            aseFile.ColorDepth, palette, aseFile.TransparentIndex);
        DrawPixelsOnSurface(surface, rgba32, celChunk.X, celChunk.Y, celChunk.Width, celChunk.Height);

        return surface;
    }

    /// <summary>
    /// Renders all visible layer cels for a frame, flattened into a single surface.
    /// </summary>
    private Surface RenderFlatFrame(AsepriteFile aseFile, int frameIndex, byte[][] palette)
    {
        var surface = new Surface(new VecI(aseFile.Width, aseFile.Height));

        if (frameIndex >= aseFile.Frames.Count)
            return surface;

        var frame = aseFile.Frames[frameIndex];

        foreach (var chunk in frame.Chunks)
        {
            if (chunk is not AsepriteCelChunk celChunk) continue;

            var actualCel = celChunk;
            if (celChunk.CelType == 1 && celChunk.LinkedFramePosition < aseFile.Frames.Count)
            {
                var linkedFrame = aseFile.Frames[celChunk.LinkedFramePosition];
                actualCel = linkedFrame.Chunks
                    .OfType<AsepriteCelChunk>()
                    .FirstOrDefault(c => c.LayerIndex == celChunk.LayerIndex);
                if (actualCel == null) continue;
            }

            byte[] pixelData = GetDecompressedPixelData(actualCel);
            if (pixelData == null || pixelData.Length == 0) continue;

            byte[] rgba32 = ConvertToRgba32(pixelData, actualCel.Width, actualCel.Height,
                aseFile.ColorDepth, palette, aseFile.TransparentIndex);
            DrawPixelsOnSurface(surface, rgba32, celChunk.X, celChunk.Y, actualCel.Width, actualCel.Height);
        }

        return surface;
    }

    private static byte[] GetDecompressedPixelData(AsepriteCelChunk cel)
    {
        return cel.CelType switch
        {
            0 => cel.RawPixelData,
            2 => AsepriteExporter.DecompressPixelData(cel.CompressedPixelData),
            _ => null
        };
    }

    /// <summary>
    /// Converts pixel data from the source color depth format to RGBA32.
    /// </summary>
    private static byte[] ConvertToRgba32(byte[] data, int width, int height, ushort colorDepth,
        byte[][] palette, byte transparentIndex)
    {
        int pixelCount = width * height;
        byte[] rgba = new byte[pixelCount * 4];

        switch (colorDepth)
        {
            case 32: // Already RGBA
                int copyLen = Math.Min(data.Length, pixelCount * 4);
                Array.Copy(data, rgba, copyLen);
                break;

            case 16: // Grayscale (Value, Alpha)
                for (int i = 0; i < pixelCount && i * 2 + 1 < data.Length; i++)
                {
                    byte value = data[i * 2];
                    byte alpha = data[i * 2 + 1];
                    rgba[i * 4] = value;
                    rgba[i * 4 + 1] = value;
                    rgba[i * 4 + 2] = value;
                    rgba[i * 4 + 3] = alpha;
                }
                break;

            case 8: // Indexed
                for (int i = 0; i < pixelCount && i < data.Length; i++)
                {
                    byte index = data[i];
                    if (index == transparentIndex)
                    {
                        // Transparent pixel
                        rgba[i * 4] = 0;
                        rgba[i * 4 + 1] = 0;
                        rgba[i * 4 + 2] = 0;
                        rgba[i * 4 + 3] = 0;
                    }
                    else if (palette != null && index < palette.Length && palette[index] != null)
                    {
                        rgba[i * 4] = palette[index][0];
                        rgba[i * 4 + 1] = palette[index][1];
                        rgba[i * 4 + 2] = palette[index][2];
                        rgba[i * 4 + 3] = palette[index].Length > 3 ? palette[index][3] : (byte)255;
                    }
                    else
                    {
                        rgba[i * 4] = 0;
                        rgba[i * 4 + 1] = 0;
                        rgba[i * 4 + 2] = 0;
                        rgba[i * 4 + 3] = 255;
                    }
                }
                break;
        }

        return rgba;
    }

    /// <summary>
    /// Draws RGBA32 pixel data onto a Surface at the given position.
    /// </summary>
    private static void DrawPixelsOnSurface(Surface surface, byte[] rgba32, int x, int y, int width, int height)
    {
        if (width <= 0 || height <= 0 || rgba32 == null || rgba32.Length == 0)
            return;

        using var celSurface = new Surface(new VecI(width, height));
        celSurface.DrawBytes(new VecI(width, height), rgba32, ColorType.Rgba8888, AlphaType.Unpremul);

        surface.DrawingSurface.Canvas.DrawSurface(celSurface.DrawingSurface, x, y);
    }

    /// <summary>
    /// Builds the palette lookup table from Aseprite palette chunks.
    /// </summary>
    private static byte[][] BuildPalette(
        AsepritePaletteChunk newPalette,
        AsepriteOldPaletteChunk0004 oldPalette,
        ushort colorCount)
    {
        int count = colorCount == 0 ? 256 : colorCount;
        var palette = new byte[count][];

        for (int i = 0; i < count; i++)
            palette[i] = new byte[] { 0, 0, 0, 255 };

        if (newPalette != null)
        {
            for (int i = 0; i < newPalette.Entries.Count; i++)
            {
                int idx = (int)newPalette.FirstColorIndex + i;
                if (idx < count)
                {
                    var entry = newPalette.Entries[i];
                    palette[idx] = new byte[] { entry.R, entry.G, entry.B, entry.A };
                }
            }
        }
        else if (oldPalette != null)
        {
            int idx = 0;
            foreach (var packet in oldPalette.Packets)
            {
                idx += packet.Skip;
                foreach (var color in packet.Colors)
                {
                    if (idx < count)
                        palette[idx] = new byte[] { color.R, color.G, color.B, 255 };
                    idx++;
                }
            }
        }

        return palette;
    }

    /// <summary>
    /// Sets the document's swatch palette from the Aseprite palette data.
    /// </summary>
    private static void SetSwatches(DocumentViewModelBuilder builder, byte[][] palette)
    {
        if (palette == null) return;

        var swatches = new List<PaletteColor>();
        foreach (var entry in palette)
        {
            if (entry != null && entry.Length >= 3)
            {
                byte a = entry.Length > 3 ? entry[3] : (byte)255;
                if (a > 0)
                    swatches.Add(new PaletteColor(entry[0], entry[1], entry[2]));
            }
        }

        if (swatches.Count > 0)
            builder.WithSwatches(swatches);
    }

    /// <summary>
    /// Maps Aseprite blend mode values to PixiEditor blend modes.
    /// </summary>
    private static ChangeableDocument.Enums.BlendMode MapBlendMode(ushort aseBlendMode)
    {
        return aseBlendMode switch
        {
            0 => ChangeableDocument.Enums.BlendMode.Normal,
            1 => ChangeableDocument.Enums.BlendMode.Multiply,
            2 => ChangeableDocument.Enums.BlendMode.Screen,
            3 => ChangeableDocument.Enums.BlendMode.Overlay,
            4 => ChangeableDocument.Enums.BlendMode.Darken,
            5 => ChangeableDocument.Enums.BlendMode.Lighten,
            6 => ChangeableDocument.Enums.BlendMode.ColorDodge,
            7 => ChangeableDocument.Enums.BlendMode.ColorBurn,
            8 => ChangeableDocument.Enums.BlendMode.HardLight,
            9 => ChangeableDocument.Enums.BlendMode.SoftLight,
            10 => ChangeableDocument.Enums.BlendMode.Difference,
            11 => ChangeableDocument.Enums.BlendMode.Exclusion,
            12 => ChangeableDocument.Enums.BlendMode.Hue,
            13 => ChangeableDocument.Enums.BlendMode.Saturation,
            14 => ChangeableDocument.Enums.BlendMode.Color,
            15 => ChangeableDocument.Enums.BlendMode.Luminosity,
            16 => ChangeableDocument.Enums.BlendMode.LinearDodge, // Addition
            _ => ChangeableDocument.Enums.BlendMode.Normal
        };
    }
}
