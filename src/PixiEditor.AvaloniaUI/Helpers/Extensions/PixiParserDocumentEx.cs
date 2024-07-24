using System.Collections;
using ChunkyImageLib;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Surfaces.ImageData;
using PixiEditor.DrawingApi.Skia;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Numerics;
using PixiEditor.Parser;
using PixiEditor.Parser.Graph;
using PixiEditor.Parser.Skia;

namespace PixiEditor.AvaloniaUI.Helpers.Extensions;

internal static class PixiParserDocumentEx
{
    public static VecD ToVecD(this Vector2 vec)
    {
        return new VecD(vec.X, vec.Y);
    }

    public static Vector2 ToVector2(this VecD vec)
    {
        return new Vector2() { X = vec.X, Y = vec.Y };
    }

    public static DocumentViewModel ToDocument(this Document document)
    {
        ImageEncoder? encoder = document.GetEncoder();
        if (encoder == null)
        {
            throw new ArgumentException("Document does not have a valid encoder");
        }

        return DocumentViewModel.Build(b => b
            .WithSize(document.Width, document.Height)
            .WithPalette(document.Palette, color => new PaletteColor(color.R, color.G, color.B))
            .WithSwatches(document.Swatches, x => new(x.R, x.G, x.B))
            .WithReferenceLayer(document.ReferenceLayer, BuildReferenceLayer, encoder)
            .WithGraph(document.Graph, encoder, BuildGraph)
            .WithAnimationData(document.AnimationData));
    }

    private static void BuildGraph(NodeGraph graph, NodeGraphBuilder graphBuilder, ImageEncoder encoder)
    {
        if (graph.AllNodes != null)
        {
            foreach (var node in graph.AllNodes)
            {
                graphBuilder.WithNode(x => x
                    .WithId(node.Id)
                    .WithPosition(node.Position)
                    .WithName(node.Name)
                    .WithUniqueNodeName(node.UniqueNodeName)
                    .WithInputValues(ParseArbitraryData(ToDictionary(node.InputPropertyValues), encoder))
                    .WithAdditionalData(ParseArbitraryData(node.AdditionalData, encoder))
                    .WithConnections(node.InputConnections));
            }
        }
    }

    private static Dictionary<string, object> ParseArbitraryData(Dictionary<string, object> data, ImageEncoder encoder)
    {
        Dictionary<string, object> parsedData = new();

        foreach (var item in data)
        {
            if (item.Value is IEnumerable enumerable)
            {
                List<object> parsedList = new();
                foreach (var listElement in enumerable)
                {
                    if (TryParseSurface(listElement, encoder, out object parsed))
                    {
                        parsedList.Add(parsed);
                    }
                }

                if (parsedList.Count > 0)
                {
                    // if all children are the same type
                    if (parsedList.All(x => x is Surface))
                    {
                        parsedData.Add(item.Key, parsedList.Cast<Surface>());
                    }
                    else
                    {
                        parsedData.Add(item.Key, parsedList);
                    }
                }
                else
                {
                    parsedData.Add(item.Key, item.Value);
                }
            }
            else if (TryParseSurface(item, encoder, out object parsed))
            {
                parsedData.Add(item.Key, parsed);
            }
            else
            {
                parsedData.Add(item.Key, item.Value);
            }
        }

        return parsedData;
    }

    private static bool TryParseSurface(object item, ImageEncoder encoder, out object parsed)
    {
        parsed = null;
        if (item is IEnumerable<object> objEnumerable)
        {
            var array = objEnumerable.ToArray();
            if (array.Count() == 3 && array.First() is IEnumerable<byte> bytes)
            {
                try
                {
                    parsed = DecodeSurface(bytes.ToArray(), encoder);
                    return true;
                }
                catch
                {
                    parsed = item;
                    return false;
                }
            }
        }

        return false;
    }

    private static Dictionary<string, object> ToDictionary(IEnumerable<NodePropertyValue> properties)
    {
        Dictionary<string, object> dict = new();
        foreach (var property in properties)
        {
            dict[property.PropertyName] = property.Value;
        }

        return dict;
    }

    private static void BuildReferenceLayer(
        ReferenceLayer referenceLayer,
        DocumentViewModelBuilder.ReferenceLayerBuilder layerBuilder,
        ImageEncoder encoder)
    {
        DecodeSurface(referenceLayer.ImageBytes, (int)referenceLayer.Width, (int)referenceLayer.Height, encoder);

        layerBuilder
            .WithIsVisible(referenceLayer.Enabled)
            .WithShape(referenceLayer.Corners)
            .WithIsTopmost(referenceLayer.Topmost)
            .WithSurface(Surface.Load(referenceLayer.ImageBytes));
    }

    private static Surface DecodeSurface(byte[] imgBytes, int width, int height, ImageEncoder encoder)
    {
        Surface surface = new Surface(new VecI(width, height));

        byte[] decoded =
            encoder.Decode(imgBytes, out SKImageInfo info);
        surface.DrawBytes(surface.Size, decoded, info.ColorType.ToColorType(), info.AlphaType.ToAlphaType());

        return surface;
    }
    
    private static Surface DecodeSurface(byte[] imgBytes, ImageEncoder encoder)
    {
        byte[] decoded =
            encoder.Decode(imgBytes, out SKImageInfo info);
        using Image img = Image.FromPixels(info.ToImageInfo(), decoded);
        Surface surface = new Surface(img.Size);
        surface.DrawingSurface.Canvas.DrawImage(img, 0, 0);

        return surface;
    }
}
