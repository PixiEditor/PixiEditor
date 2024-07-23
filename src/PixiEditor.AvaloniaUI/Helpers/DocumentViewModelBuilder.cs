using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.AvaloniaUI.Helpers.Extensions;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Numerics;
using PixiEditor.Parser;
using PixiEditor.Parser.Graph;
using PixiEditor.Parser.Skia;

namespace PixiEditor.AvaloniaUI.Helpers;

internal class DocumentViewModelBuilder : PixiParserV3DocumentEx.ChildrenBuilder
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

    public DocumentViewModelBuilder WithReferenceLayer<T>(T reference, Action<T, ReferenceLayerBuilder, ImageEncoder?> builder,
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
    
    public DocumentViewModelBuilder WithGraph(NodeGraph graph)
    {
        Graph = new NodeGraphBuilder();
        
        if (graph.AllNodes != null)
        {
            foreach (var node in graph.AllNodes)
            {
                Graph.WithNode(new NodeBuilder()
                    .WithId(node.Id)
                    .WithPosition(node.Position)
                    .WithName(node.Name)
                    .WithUniqueNodeName(node.UniqueNodeName));
            }
        }
        
        return this;
    }

    private static void BuildKeyFrames(List<IKeyFrame> root, List<KeyFrameBuilder> data)
    {
        foreach (var keyFrame in root)
        {
            if (keyFrame is KeyFrameGroup group)
            {
                GroupKeyFrameBuilder builder = new GroupKeyFrameBuilder()
                    .WithVisibility(group.Enabled)
                    .WithId(group.LayerGuid)
                    .WithLayerGuid(group.LayerGuid);

                foreach (var child in group.Children)
                {
                    if(child is KeyFrameGroup childGroup)
                    {
                        builder.WithChild<GroupKeyFrameBuilder>(x => BuildKeyFrames(childGroup.Children, null, documentRootFolder));
                    }
                    else if (child is RasterKeyFrame rasterKeyFrame)
                    {
                        builder.WithChild<RasterKeyFrameBuilder>(x => x
                            .WithVisibility(builder.IsVisible)
                            .WithId(rasterKeyFrame.Guid)
                            .WithLayerGuid(rasterKeyFrame.LayerGuid)
                            .WithStartFrame(rasterKeyFrame.StartFrame)
                            .WithDuration(rasterKeyFrame.Duration)
                            .WithSurface(Surface.Load(rasterKeyFrame.ImageBytes)));
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
    public Guid LayerGuid { get; set; }
    public Guid Id { get; set; }

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

    public KeyFrameBuilder WithLayerGuid(Guid layerGuid)
    {
        LayerGuid = layerGuid;
        return this;
    }

    public KeyFrameBuilder WithId(Guid id)
    {
        Id = id;
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
    
    public new GroupKeyFrameBuilder WithVisibility(bool isVisible) => base.WithVisibility(isVisible) as GroupKeyFrameBuilder;
    public new GroupKeyFrameBuilder WithLayerGuid(Guid layerGuid) => base.WithLayerGuid(layerGuid) as GroupKeyFrameBuilder;
    public new GroupKeyFrameBuilder WithId(Guid id) => base.WithId(id) as GroupKeyFrameBuilder;
    public new GroupKeyFrameBuilder WithStartFrame(int startFrame) => base.WithStartFrame(startFrame) as GroupKeyFrameBuilder;
    public new GroupKeyFrameBuilder WithDuration(int duration) => base.WithDuration(duration) as GroupKeyFrameBuilder;
}

internal class RasterKeyFrameBuilder : KeyFrameBuilder
{
    public new RasterKeyFrameBuilder WithVisibility(bool isVisible) => base.WithVisibility(isVisible) as RasterKeyFrameBuilder;
    public new RasterKeyFrameBuilder WithLayerGuid(Guid layerGuid) => base.WithLayerGuid(layerGuid) as RasterKeyFrameBuilder;
    public new RasterKeyFrameBuilder WithId(Guid id) => base.WithId(id) as RasterKeyFrameBuilder;
    public new RasterKeyFrameBuilder WithStartFrame(int startFrame) => base.WithStartFrame(startFrame) as RasterKeyFrameBuilder;
    public new RasterKeyFrameBuilder WithDuration(int duration) => base.WithDuration(duration) as RasterKeyFrameBuilder;
}

internal class NodeGraphBuilder
{
    public List<NodeBuilder> AllNodes { get; set; } = new List<NodeBuilder>();

    public NodeGraphBuilder WithNode(NodeBuilder node)
    {
        AllNodes.Add(node);
        return this;
    }
}

internal class NodeBuilder
{
    public int Id { get; set; }
    public Vector2 Position { get; set; }
    public string Name { get; set; }
    public string UniqueNodeName { get; set; }

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
}
