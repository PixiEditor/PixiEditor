using System.Diagnostics.CodeAnalysis;
using ChunkyImageLib;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Numerics;
using PixiEditor.Parser;
using PixiEditor.Parser.Old.PixiV4;
using BlendMode = PixiEditor.Parser.BlendMode;

namespace PixiEditor.AvaloniaUI.Helpers.Extensions;

internal static class PixiParserPixiV4DocumentEx
{
    public static DocumentViewModel ToDocument(this DocumentV4 document)
    {
        // TODO: Implement?
        return DocumentViewModel.Build(b =>
        {
            /*b.WithSize(document.Width, document.Height)
                .WithPalette(document.Palette, x => new PaletteColor(x.R, x.G, x.B))
                .WithSwatches(document.Swatches, x => new(x.R, x.G, x.B))
                .WithReferenceLayer(document.ReferenceLayer, (r, builder) => builder
                    .WithIsVisible(r.Enabled)
                    .WithShape(r.Corners)
                    .WithIsTopmost(r.Topmost)
                    .WithSurface(Surface.Load(r.ImageBytes)));

            BuildChildren(b, document.RootFolder.Children);*/
        });

        void BuildChildren(ChildrenBuilder builder, IEnumerable<IStructureMember> members)
        {
            foreach (var member in members)
            {
                if (member is Folder folder)
                {
                    builder.WithFolder(x => BuildFolder(x, folder));
                }
                else if (member is ImageLayer layer)
                {
                    builder.WithLayer(x => BuildLayer(x, layer));
                }
                else
                {
                    throw new NotImplementedException(
                        $"StructureMember of type '{member.GetType().FullName}' has not been implemented");
                }
            }
        }

        void BuildFolder(FolderBuilder builder, Folder folder) => builder
            .WithName(folder.Name)
            .WithVisibility(folder.Enabled)
            .WithOpacity(folder.Opacity)
            .WithBlendMode(folder.BlendMode)
            .WithChildren(x => BuildChildren(x, folder.Children))
            .WithClipToBelow(folder.ClipToMemberBelow)
            .WithMask(folder.Mask,
                (x, m) => x.WithVisibility(m.Enabled).WithSurface(m.Width, m.Height,
                    x => x.WithImage(m.ImageBytes, m.OffsetX, m.OffsetY)));

        void BuildLayer(LayerBuilder builder, ImageLayer layer)
        {
            builder
                .WithName(layer.Name)
                .WithGuid(layer.Guid)
                .WithVisibility(layer.Enabled)
                .WithOpacity(layer.Opacity)
                .WithBlendMode(layer.BlendMode)
                .WithRect(layer.Width, layer.Height, layer.OffsetX, layer.OffsetY)
                .WithClipToBelow(layer.ClipToMemberBelow)
                .WithLockAlpha(layer.LockAlpha)
                .WithMask(layer.Mask,
                    (x, m) => x.WithVisibility(m.Enabled).WithSurface(m.Width, m.Height,
                        x => x.WithImage(m.ImageBytes, m.OffsetX, m.OffsetY)));

            if (layer is { Width: > 0, Height: > 0 })
            {
                builder.WithSurface(x => x.WithImage(layer.ImageBytes, 0, 0));
            }
        }
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
