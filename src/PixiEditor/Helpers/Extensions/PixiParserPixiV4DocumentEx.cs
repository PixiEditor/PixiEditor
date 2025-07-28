using System.Diagnostics.CodeAnalysis;
using ChunkyImageLib;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using PixiEditor.Extensions.CommonApi.Palettes;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Models.Serialization.Factories;
using PixiEditor.Parser;
using PixiEditor.Parser.Graph;
using PixiEditor.Parser.Old.PixiV4;
using PixiEditor.Parser.Skia.Encoders;
using PixiEditor.ViewModels.Document;
using BlendMode = PixiEditor.Parser.BlendMode;

namespace PixiEditor.Helpers.Extensions;

internal static class PixiParserPixiV4DocumentEx
{
    public static DocumentViewModel ToDocument(this DocumentV4 document)
    {
        return DocumentViewModel.Build(b =>
        {
            b.ImageEncoderUsed = "PNG";
            b.PixiParserVersionUsed = document.Version;
            b.WithSize(document.Width, document.Height)
                .WithPalette(document.Palette, x => new PaletteColor(x.R, x.G, x.B))
                .WithSwatches(document.Swatches, x => new(x.R, x.G, x.B))
                .WithReferenceLayer(document.ReferenceLayer, (r, builder, encoder) => builder
                        .WithIsVisible(r.Enabled)
                        .WithShape(r.Corners)
                        .WithIsTopmost(r.Topmost)
                        .WithSurface(Surface.Load(r.ImageBytes)),
                    new PngEncoder());

            b.WithGraph(graphBuilder =>
            {
                int lastIndex = GetIndexOfMember(document.RootFolder.Children[^1], document.RootFolder.Children);
                graphBuilder.WithNodeOfType(typeof(OutputNode)).WithId(-1)
                    .WithConnections([
                        new PropertyConnection()
                        {
                            InputPropertyName = OutputNode.InputPropertyName,
                            OutputNodeId = lastIndex,
                            OutputPropertyName = "Output"
                        }
                    ]);

                BuildChildren(graphBuilder, document.RootFolder.Children, OutputNode.InputPropertyName);
            });
        });

        void BuildChildren(NodeGraphBuilder builder, IList<IStructureMember> members, string inputProperty)
        {
            for (var i = members.Count - 1; i >= 0; i--)
            {
                var member = members[i];
                if (member is Folder folder)
                {
                    builder.WithNode(nodeBuilder =>
                    {
                        int indexOfFolder = GetIndexOfMember(folder, document.RootFolder.Children);
                        BuildFolder(nodeBuilder, folder, indexOfFolder);
                        inputProperty = OutputNode.InputPropertyName;
                    });

                    if (folder.Children.Count > 0)
                    {
                        BuildChildren(builder, folder.Children, FolderNode.ContentInternalName);
                    }
                }
                else if (member is ImageLayer layer)
                {
                    builder.WithNode(nodeBuilder =>
                    {
                        int indexOfLayer = GetIndexOfMember(layer, document.RootFolder.Children);
                        BuildLayer(nodeBuilder, layer, indexOfLayer);
                        inputProperty = OutputNode.InputPropertyName;
                    });
                }
                else
                {
                    throw new NotImplementedException(
                        $"StructureMember of type '{member.GetType().FullName}' has not been implemented");
                }
            }
        }

        void BuildFolder(NodeGraphBuilder.NodeBuilder builder, Folder folder, int structureIndex)
        {
            Dictionary<string, object> inputValues = new Dictionary<string, object>();
            inputValues["IsVisible"] = folder.Enabled;
            inputValues["Opacity"] = folder.Opacity;
            inputValues["BlendMode"] = (int)folder.BlendMode;
            
            Dictionary<string, object> additionalValues = new Dictionary<string, object>();
            additionalValues["clipToPreviousMember"] = folder.ClipToMemberBelow;
            
            if (folder.Mask is not null)
            {
                inputValues["MaskIsVisible"] = folder.Mask.Enabled;
                additionalValues["embeddedMask"] = new ChunkyImage(ConvertToNewMaskFormat(Surface.Load(folder.Mask.ImageBytes), document.Width, document.Height), ColorSpace.CreateSrgb());
            }

            PropertyConnection contentConnection = new PropertyConnection()
            {
                InputPropertyName = FolderNode.ContentInternalName,
                OutputNodeId = structureIndex - 1,
                OutputPropertyName = "Output"
            };

            int childrenCount = CountChildren(folder.Children);
            int previousIndex = structureIndex - childrenCount - 1;
            PropertyConnection backgroundConnection = new PropertyConnection()
            {
                InputPropertyName = OutputNode.InputPropertyName,
                OutputNodeId = previousIndex,
                OutputPropertyName = "Output"
            };
            
            builder
                .WithId(structureIndex)
                .WithName(folder.Name)
                .WithUniqueNodeName("PixiEditor.Folder")
                .WithInputValues(inputValues)
                .WithAdditionalData(additionalValues)
                .WithConnections([contentConnection, backgroundConnection]);
        }

        void BuildLayer(NodeGraphBuilder.NodeBuilder builder, ImageLayer layer, int structureIndex)
        {
            Dictionary<string, object> inputValues = new Dictionary<string, object>();
            inputValues["IsVisible"] = layer.Enabled;
            inputValues["Opacity"] = layer.Opacity;
            inputValues["BlendMode"] = (int)layer.BlendMode;

            Dictionary<string, object> additionalValues = new Dictionary<string, object>();
            additionalValues["clipToPreviousMember"] = layer.ClipToMemberBelow;

            if (layer.Mask is not null)
            {
                inputValues["MaskIsVisible"] = layer.Mask.Enabled;
                additionalValues["embeddedMask"] = new ChunkyImage(ConvertToNewMaskFormat(Surface.Load(layer.Mask.ImageBytes), document.Width, document.Height), ColorSpace.CreateSrgb()); 
            }

            PropertyConnection connection = new PropertyConnection()
            {
                InputPropertyName = OutputNode.InputPropertyName, OutputNodeId = structureIndex - 1, OutputPropertyName = "Output"
            };

            builder
                .WithId(structureIndex)
                .WithName(layer.Name)
                .WithUniqueNodeName("PixiEditor.ImageLayer")
                .WithInputValues(inputValues)
                .WithAdditionalData(additionalValues)
                .WithKeyFrames(new[]
                {
                    new KeyFrameData()
                    {
                        AffectedElement = ImageLayerNode.ImageLayerKey,
                        Data = layer is { Width: > 0, Height: > 0, ImageBytes.Length: > 0 }
                            ? new ChunkyImage(Surface.Load(layer.ImageBytes), ColorSpace.CreateSrgb()) :
                            new ChunkyImage(new VecI(document.Width, document.Height), ColorSpace.CreateSrgb()),
                        Duration = 0,
                        StartFrame = 0,
                        IsVisible = true
                    }
                })
                .WithConnections([connection]);

            /*if (layer is { Width: > 0, Height: > 0 })
            {
                builder.WithSurface(x => x.WithImage(layer.ImageBytes, 0, 0));
            }*/
        }
    }
    
    private static Surface ConvertToNewMaskFormat(Surface surface, int width, int height)
    {
        // convert opaque pixels to white and transparent pixels to black
        var newSurface = new Surface(new VecI(width, height));
        newSurface.DrawingSurface.Canvas.Clear(Colors.Black);
        using ColorFilter colorFilter = ColorFilter.CreateBlendMode(Colors.White, Drawie.Backend.Core.Surfaces.BlendMode.SrcATop);
        using Paint paint = new()
        {
            Color = Colors.White,
            BlendMode = Drawie.Backend.Core.Surfaces.BlendMode.SrcOver,
            ColorFilter = colorFilter
        };
        
        newSurface.DrawingSurface.Canvas.DrawSurface(surface.DrawingSurface, 0, 0, paint);
        surface.Dispose();
        return newSurface;
    }

    private static int GetIndexOfMember(IStructureMember structureMember, IList<IStructureMember> members)
    {
        int index = 0;
        bool found = GetIndexOfMember(structureMember, members, ref index);
        return found ? index : -1;
    }

    private static bool GetIndexOfMember(IStructureMember structureMember, IList<IStructureMember> members,
        ref int index)
    {
        for (int i = 0; i < members.Count; i++)
        {
            var member = members[i];

            if (member is Folder folder1)
            {
                bool found = GetIndexOfMember(structureMember, folder1.Children, ref index);
                if (found) return true;
            }

            if (member == structureMember)
            {
                return true;
            }

            index++;
        }

        return false;
    }
    
    private static int CountChildren(IList<IStructureMember> children)
    {
        int count = 0;
        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i];
            if (child is Folder folder)
            {
                count += CountChildren(folder.Children);
            }

            count++;
        }

        return count;
    }

    internal class ChildrenBuilder
    {
        public List<StructureMemberBuilder> Children { get; set; } =
            new List<StructureMemberBuilder>();

        public ChildrenBuilder WithLayer(Action<LayerBuilder> layer)
        {
            var layerBuilder = new LayerBuilder();
            layer(layerBuilder);
            Children.Add(layerBuilder);
            return this;
        }

        public ChildrenBuilder WithFolder(Action<FolderBuilder> folder)
        {
            var folderBuilder = new FolderBuilder();
            folder(folderBuilder);
            Children.Add(folderBuilder);
            return this;
        }
    }


    public abstract class StructureMemberBuilder
    {
        private MaskBuilder maskBuilder;

        public int OrderInStructure { get; set; }

        public string Name { get; set; }

        public bool IsVisible { get; set; }

        public float Opacity { get; set; }

        public BlendMode BlendMode { get; set; }

        public bool ClipToMemberBelow { get; set; }

        public bool HasMask => maskBuilder is not null;

        [NotNull] public MaskBuilder Mask => maskBuilder ??= new MaskBuilder();

        public Guid Id { get; set; }

        public StructureMemberBuilder()
        {
            IsVisible = true;
            Opacity = 1;
        }

        public StructureMemberBuilder WithOrderInStructure(int order)
        {
            OrderInStructure = order;
            return this;
        }

        public StructureMemberBuilder WithName(string name)
        {
            Name = name;
            return this;
        }

        public StructureMemberBuilder WithVisibility(bool visibility)
        {
            IsVisible = visibility;
            return this;
        }

        public StructureMemberBuilder WithOpacity(float opacity)
        {
            Opacity = opacity;
            return this;
        }

        public StructureMemberBuilder WithBlendMode(BlendMode blendMode)
        {
            BlendMode = blendMode;
            return this;
        }

        public StructureMemberBuilder WithMask(Action<MaskBuilder> mask)
        {
            mask(Mask);
            return this;
        }

        public StructureMemberBuilder WithMask<T>(T reference, Action<MaskBuilder, T> mask)
        {
            return reference != null ? WithMask(x => mask(x, reference)) : this;
        }

        public StructureMemberBuilder WithGuid(Guid guid)
        {
            Id = guid;
            return this;
        }

        public StructureMemberBuilder WithClipToBelow(bool value)
        {
            ClipToMemberBelow = value;
            return this;
        }
    }

    public class LayerBuilder : StructureMemberBuilder
    {
        private int? width;
        private int? height;

        public SurfaceBuilder? Surface { get; set; }

        public int Width
        {
            get => width ?? default;
            set => width = value;
        }

        public int Height
        {
            get => height ?? default;
            set => height = value;
        }

        public int OffsetX { get; set; }

        public int OffsetY { get; set; }

        public bool LockAlpha { get; set; }

        public new LayerBuilder WithName(string name) => base.WithName(name) as LayerBuilder;

        public new LayerBuilder WithVisibility(bool visibility) => base.WithVisibility(visibility) as LayerBuilder;

        public new LayerBuilder WithOpacity(float opacity) => base.WithOpacity(opacity) as LayerBuilder;

        public new LayerBuilder WithBlendMode(BlendMode blendMode) => base.WithBlendMode(blendMode) as LayerBuilder;

        public new LayerBuilder WithClipToBelow(bool value) => base.WithClipToBelow(value) as LayerBuilder;

        public LayerBuilder WithLockAlpha(bool layerLockAlpha)
        {
            LockAlpha = layerLockAlpha;
            return this;
        }

        public new LayerBuilder WithMask(Action<MaskBuilder> mask) => base.WithMask(mask) as LayerBuilder;

        public new LayerBuilder WithGuid(Guid guid) => base.WithGuid(guid) as LayerBuilder;

        public LayerBuilder WithSurface(Surface surface)
        {
            Surface = new(surface);
            return this;
        }

        public LayerBuilder WithSize(int width, int height)
        {
            Width = width;
            Height = height;
            return this;
        }

        public LayerBuilder WithSize(VecI size) => WithSize(size.X, size.Y);

        public LayerBuilder WithRect(int width, int height, int offsetX, int offsetY)
        {
            Width = width;
            Height = height;
            OffsetX = offsetX;
            OffsetY = offsetY;
            return this;
        }

        public LayerBuilder WithSurface(Action<SurfaceBuilder> surface)
        {
            if (width is null || height is null)
            {
                throw new InvalidOperationException(
                    "You must first set the width and height of the layer. You can do this by calling WithRect() or setting the Width and Height properties.");
            }

            var surfaceBuilder = new SurfaceBuilder(new Surface(new VecI(Width, Height)));
            surface(surfaceBuilder);
            Surface = surfaceBuilder;
            return this;
        }
    }

    public class FolderBuilder : StructureMemberBuilder
    {
        public List<StructureMemberBuilder> Children { get; set; } = new List<StructureMemberBuilder>();

        public new FolderBuilder WithName(string name) => base.WithName(name) as FolderBuilder;

        public new FolderBuilder WithVisibility(bool visibility) => base.WithVisibility(visibility) as FolderBuilder;

        public new FolderBuilder WithOpacity(float opacity) => base.WithOpacity(opacity) as FolderBuilder;

        public new FolderBuilder WithBlendMode(BlendMode blendMode) => base.WithBlendMode(blendMode) as FolderBuilder;

        public new FolderBuilder WithMask(Action<MaskBuilder> mask) => base.WithMask(mask) as FolderBuilder;

        public new FolderBuilder WithGuid(Guid guid) => base.WithGuid(guid) as FolderBuilder;

        public FolderBuilder WithClipToBelow(bool value) => base.WithClipToBelow(value) as FolderBuilder;

        public FolderBuilder WithChildren(Action<ChildrenBuilder> children)
        {
            ChildrenBuilder childrenBuilder = new();
            children(childrenBuilder);
            Children = childrenBuilder.Children;
            return this;
        }
    }

    public class SurfaceBuilder
    {
        public Surface Surface { get; set; }

        public SurfaceBuilder(Surface surface)
        {
            Surface = surface;
        }

        public SurfaceBuilder WithImage(ReadOnlySpan<byte> buffer) => WithImage(buffer, 0, 0);

        public SurfaceBuilder WithImage(ReadOnlySpan<byte> buffer, int x, int y)
        {
            if (buffer.IsEmpty) return this;

            Surface.DrawingSurface.Canvas.DrawBitmap(Bitmap.Decode(buffer), x, y);
            return this;
        }
    }

    public class MaskBuilder
    {
        public bool IsVisible { get; set; }

        public SurfaceBuilder Surface { get; set; }

        public MaskBuilder()
        {
            IsVisible = true;
        }

        public MaskBuilder WithVisibility(bool isVisible)
        {
            IsVisible = isVisible;
            return this;
        }

        public MaskBuilder WithSurface(Surface surface)
        {
            Surface = new SurfaceBuilder(surface);
            return this;
        }

        public MaskBuilder WithSurface(int width, int height, Action<SurfaceBuilder> surface)
        {
            var surfaceBuilder = new SurfaceBuilder(new Surface(new VecI(Math.Max(width, 1), Math.Max(height, 1))));
            surface(surfaceBuilder);
            Surface = surfaceBuilder;
            return this;
        }
    }

    internal class RasterKeyFrameBuilder : KeyFrameBuilder
    {
        /*public new RasterKeyFrameBuilder WithVisibility(bool isVisible) =>
            base.WithVisibility(isVisible) as RasterKeyFrameBuilder;*/

        public new RasterKeyFrameBuilder WithLayerGuid(int layerId) =>
            base.WithKeyFrameId(layerId) as RasterKeyFrameBuilder;

        /*public new RasterKeyFrameBuilder WithStartFrame(int startFrame) =>
            base.WithStartFrame(startFrame) as RasterKeyFrameBuilder;

        public new RasterKeyFrameBuilder WithDuration(int duration) =>
            base.WithDuration(duration) as RasterKeyFrameBuilder;*/
    }
}
