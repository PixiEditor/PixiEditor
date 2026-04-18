using Drawie.Backend.Core;
using PixiEditor.Extensions.CommonApi.Palettes;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.Models.IO;
using PixiEditor.Parser;
using PixiEditor.Parser.Graph;
using PixiEditor.Parser.Skia;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Helpers.Extensions;

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
            .WithPixiParserVersion(document.Version)
            .WithSerializerData(document.SerializerName, document.SerializerVersion)
            .WithSrgbColorBlending(document.SrgbColorBlending)
            .WithSize(document.Width, document.Height)
            .WithImageEncoder(document.ImageEncoderUsed)
            .WithPalette(document.Palette, color => new PaletteColor(color.R, color.G, color.B))
            .WithSwatches(document.Swatches, x => new(x.R, x.G, x.B))
            .WithReferenceLayer(document.ReferenceLayer, BuildReferenceLayer, encoder)
            .WithGraph(document.Graph, BuildGraph)
            .WithAnimationData(document.AnimationData, document.Graph)
            .WithResources(document.Resources));
    }

    private static void BuildGraph(NodeGraph graph, NodeGraphBuilder graphBuilder)
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
                    .WithKeyFrames(node.KeyFrames)
                    .WithInputValues(ToDictionary(node.InputPropertyValues))
                    .WithAdditionalData(node.AdditionalData)
                    .WithPairId(node.PairId)
                    .WithConnections(node.InputConnections));
            }

            if (graph.Blackboard != null)
            {
                graphBuilder.WithBlackboard(x =>
                {
                    foreach (var kvp in graph.Blackboard.Variables)
                    {
                        x.WithVariable(kvp.Name, kvp.Value, kvp.Type, kvp.Unit, kvp.Min, kvp.Max, kvp.IsExposed);
                    }
                });
            }
        }
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
        var surface = DecodeSurface(referenceLayer.ImageBytes, referenceLayer.ImageWidth, referenceLayer.ImageHeight, encoder);

        layerBuilder
            .WithIsVisible(referenceLayer.Enabled)
            .WithShape(referenceLayer.Corners)
            .WithIsTopmost(referenceLayer.Topmost)
            .WithSurface(surface);
    }

    private static Surface DecodeSurface(byte[] imgBytes, int width, int height, ImageEncoder encoder)
    {
        Surface surface = new Surface(new VecI(width, height));

        byte[] decoded =
            encoder.Decode(imgBytes, out SKImageInfo info);
        surface.DrawBytes(surface.Size, decoded, info.ColorType.ToColorType(), info.AlphaType.ToAlphaType());

        return surface;
    }
}
