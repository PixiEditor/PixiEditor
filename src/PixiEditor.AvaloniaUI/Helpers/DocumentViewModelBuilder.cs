using System.Collections;
using System.Reflection;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.AvaloniaUI.Helpers.Extensions;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.DrawingApi.Core;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Numerics;
using PixiEditor.Parser;
using PixiEditor.Parser.Graph;
using PixiEditor.Parser.Skia;
using NodeGraph = PixiEditor.Parser.Graph.NodeGraph;

namespace PixiEditor.AvaloniaUI.Helpers;

internal class DocumentViewModelBuilder
{
    public int Width { get; set; }
    public int Height { get; set; }

    public List<PaletteColor> Swatches { get; set; } = new List<PaletteColor>();
    public List<PaletteColor> Palette { get; set; } = new List<PaletteColor>();

    public ReferenceLayerBuilder ReferenceLayer { get; set; }
    public List<KeyFrameBuilder> AnimationData { get; set; } = new List<KeyFrameBuilder>();

    public NodeGraphBuilder Graph { get; set; }

    public DocumentViewModelBuilder WithSize(int width, int height)
    {
        Width = width;
        Height = height;

        return this;
    }

    public DocumentViewModelBuilder WithSize(VecI size) => WithSize(size.X, size.Y);

    public DocumentViewModelBuilder WithSwatches(IEnumerable<PaletteColor> swatches)
    {
        Swatches = new(swatches);
        return this;
    }

    public DocumentViewModelBuilder WithSwatches<T>(IEnumerable<T> swatches, Func<T, PaletteColor> toColor) =>
        WithSwatches(swatches.Select(toColor));

    public DocumentViewModelBuilder WithPalette(IEnumerable<PaletteColor> palette)
    {
        Palette = new(palette);
        return this;
    }

    public DocumentViewModelBuilder WithPalette<T>(IEnumerable<T> pallet, Func<T, PaletteColor> toColor) =>
        WithPalette(pallet.Select(toColor));

    public DocumentViewModelBuilder WithReferenceLayer<T>(T reference,
        Action<T, ReferenceLayerBuilder, ImageEncoder?> builder,
        ImageEncoder? encoder)
    {
        if (reference != null)
        {
            WithReferenceLayer(x => builder(reference, x, encoder));
        }

        return this;
    }

    public DocumentViewModelBuilder WithReferenceLayer(Action<ReferenceLayerBuilder> builder)
    {
        var reference = new ReferenceLayerBuilder();

        builder(reference);

        ReferenceLayer = reference;

        return this;
    }

    public DocumentViewModelBuilder WithAnimationData(AnimationData? animationData)
    {
        AnimationData = new List<KeyFrameBuilder>();

        if (animationData != null && animationData.KeyFrameGroups.Count > 0)
        {
            BuildKeyFrames(animationData.KeyFrameGroups.Cast<IKeyFrame>().ToList(), AnimationData);
        }

        return this;
    }

    public DocumentViewModelBuilder WithGraph(NodeGraph graph, ImageEncoder encoder, Action<NodeGraph, NodeGraphBuilder, ImageEncoder> builder)
    {
        if (graph != null)
        {
            WithGraph(x => builder(graph, x, encoder));
        }

        return this;
    }

    public DocumentViewModelBuilder WithGraph(Action<NodeGraphBuilder> builder)
    {
        var graph = new NodeGraphBuilder();
        builder(graph);
        Graph = graph;
        return this;
    }

    private static Dictionary<string, object> ToDictionary(NodePropertyValue[] values)
    {
        Dictionary<string, object> dictionary = new();

        foreach (var value in values)
        {
            dictionary.Add(value.PropertyName, value.Value);
        }

        return dictionary;
    }

    private static void BuildKeyFrames(List<IKeyFrame> root, List<KeyFrameBuilder> data)
    {
        foreach (var keyFrame in root)
        {
            if (keyFrame is KeyFrameGroup group)
            {
                GroupKeyFrameBuilder builder = new GroupKeyFrameBuilder()
                    .WithVisibility(group.Enabled)
                    .WithNodeId(group.NodeId);

                foreach (var child in group.Children)
                {
                    if (child is KeyFrameGroup childGroup)
                    {
                        builder.WithChild<GroupKeyFrameBuilder>(x =>
                            BuildKeyFrames(childGroup.Children, null));
                    }
                    else if (child is RasterKeyFrame rasterKeyFrame)
                    {
                        builder.WithChild<RasterKeyFrameBuilder>(x => x
                            .WithVisibility(builder.IsVisible)
                            .WithLayerGuid(rasterKeyFrame.NodeId)
                            .WithStartFrame(rasterKeyFrame.StartFrame)
                            .WithDuration(rasterKeyFrame.Duration));
                    }
                }

                data?.Add(builder);
            }
        }
    }

    public class ReferenceLayerBuilder
    {
        public bool IsVisible { get; set; }

        public bool IsTopmost { get; set; }

        public VecI ImageSize { get; set; }

        public ShapeCorners Shape { get; set; }

        public byte[] ImageBgra8888Bytes { get; set; }

        public ReferenceLayerBuilder WithIsVisible(bool isVisible)
        {
            IsVisible = isVisible;
            return this;
        }

        public ReferenceLayerBuilder WithIsTopmost(bool isTopmost)
        {
            IsTopmost = isTopmost;
            return this;
        }

        public ReferenceLayerBuilder WithSurface(Surface surface)
        {
            byte[] bytes = surface.ToByteArray();
            WithImage(surface.Size, bytes);

            return this;
        }

        public ReferenceLayerBuilder WithImage(VecI size, byte[] pbgraData)
        {
            ImageSize = size;
            ImageBgra8888Bytes = pbgraData;
            return this;
        }

        public ReferenceLayerBuilder WithShape(Corners rect)
        {
            Shape = new ShapeCorners
            {
                TopLeft = rect.TopLeft.ToVecD(),
                TopRight = rect.TopRight.ToVecD(),
                BottomLeft = rect.BottomLeft.ToVecD(),
                BottomRight = rect.BottomRight.ToVecD()
            };

            return this;
        }
    }
}

internal class KeyFrameBuilder()
{
    public int StartFrame { get; set; }
    public int Duration { get; set; }
    public bool IsVisible { get; set; }
    public int LayerId { get; set; }

    public KeyFrameBuilder WithStartFrame(int startFrame)
    {
        StartFrame = startFrame;
        return this;
    }

    public KeyFrameBuilder WithDuration(int duration)
    {
        Duration = duration;
        return this;
    }

    public KeyFrameBuilder WithVisibility(bool isVisible)
    {
        IsVisible = isVisible;
        return this;
    }

    public KeyFrameBuilder WithLayerGuid(int layerGuid)
    {
        LayerId = layerGuid;
        return this;
    }
}

internal class GroupKeyFrameBuilder : KeyFrameBuilder
{
    public List<KeyFrameBuilder> Children { get; set; } = new List<KeyFrameBuilder>();

    public GroupKeyFrameBuilder WithChild<T>(Action<T> child) where T : KeyFrameBuilder, new()
    {
        var childBuilder = new T();
        child(childBuilder);
        Children.Add(childBuilder);
        return this;
    }

    public new GroupKeyFrameBuilder WithVisibility(bool isVisible) =>
        base.WithVisibility(isVisible) as GroupKeyFrameBuilder;

    public new GroupKeyFrameBuilder WithNodeId(int layerGuid) =>
        base.WithLayerGuid(layerGuid) as GroupKeyFrameBuilder;

    public new GroupKeyFrameBuilder WithStartFrame(int startFrame) =>
        base.WithStartFrame(startFrame) as GroupKeyFrameBuilder;

    public new GroupKeyFrameBuilder WithDuration(int duration) => base.WithDuration(duration) as GroupKeyFrameBuilder;
}

internal class RasterKeyFrameBuilder : KeyFrameBuilder
{
    public new RasterKeyFrameBuilder WithVisibility(bool isVisible) =>
        base.WithVisibility(isVisible) as RasterKeyFrameBuilder;

    public new RasterKeyFrameBuilder WithLayerGuid(int layerId) =>
        base.WithLayerGuid(layerId) as RasterKeyFrameBuilder;

    public new RasterKeyFrameBuilder WithStartFrame(int startFrame) =>
        base.WithStartFrame(startFrame) as RasterKeyFrameBuilder;

    public new RasterKeyFrameBuilder WithDuration(int duration) => base.WithDuration(duration) as RasterKeyFrameBuilder;
}

internal class NodeGraphBuilder
{
    public List<NodeBuilder> AllNodes { get; set; } = new List<NodeBuilder>();


    public NodeGraphBuilder WithNode(Action<NodeBuilder> nodeBuilder)
    {
        var node = new NodeBuilder();
        nodeBuilder(node);

        AllNodes.Add(node);

        return this;
    }

    public NodeGraphBuilder WithOutputNode(int? toConnectNodeId, string? toConnectPropName)
    {
        var node = this.WithNodeOfType(typeof(OutputNode))
            .WithId(AllNodes.Count + 1);

        if (toConnectNodeId != null && toConnectPropName != null)
        {
            node.WithConnections(new[]
            {
                new PropertyConnection
                {
                    OutputNodeId = toConnectNodeId.Value,
                    OutputPropertyName = toConnectPropName,
                    InputPropertyName = OutputNode.InputPropertyName
                }
            });
        }

        AllNodes.Add(node);
        return this;
    }

    public NodeGraphBuilder WithImageLayerNode(string name, Surface image, out int id)
    {
        AllNodes.Add(
            this.WithNodeOfType(typeof(ImageLayerNode))
                .WithName(name)
                .WithId(AllNodes.Count + 1)
                .WithAdditionalData(
                    new Dictionary<string, object> { { ImageLayerNode.ImageFramesKey, new List<Surface> { image } } }));

        id = AllNodes.Count;
        return this;
    }

    public NodeBuilder WithNodeOfType(Type nodeType)
    {
        var node = new NodeBuilder();
        node.WithUniqueNodeName(nodeType.GetCustomAttribute<NodeInfoAttribute>().UniqueName);

        return node;
    }

    internal class NodeBuilder
    {
        public int Id { get; set; }
        public Vector2 Position { get; set; }
        public string Name { get; set; }
        public string UniqueNodeName { get; set; }
        public Dictionary<string, object> InputValues { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; }
        public Dictionary<int, (string inputPropName, string outputPropName)> InputConnections { get; set; }

        public NodeBuilder WithId(int id)
        {
            Id = id;
            return this;
        }

        public NodeBuilder WithPosition(Vector2 position)
        {
            Position = position;
            return this;
        }

        public NodeBuilder WithName(string name)
        {
            Name = name;
            return this;
        }

        public NodeBuilder WithUniqueNodeName(string uniqueNodeName)
        {
            UniqueNodeName = uniqueNodeName;
            return this;
        }

        public NodeBuilder WithInputValues(Dictionary<string, object> values)
        {
            InputValues = values;
            return this;
        }

        public NodeBuilder WithAdditionalData(Dictionary<string, object> data)
        {
            AdditionalData = data;
            return this;
        }

        public NodeBuilder WithConnections(PropertyConnection[] nodeInputConnections)
        {
            InputConnections = new Dictionary<int, (string, string)>();

            foreach (var connection in nodeInputConnections)
            {
                InputConnections.Add(connection.OutputNodeId,
                    (connection.InputPropertyName, connection.OutputPropertyName));
            }

            return this;
        }
    }
}
