using System.Diagnostics.CodeAnalysis;
using System.IO;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.Helpers;

internal class DocumentViewModelBuilder : ChildrenBuilder
{
    public int Width { get; set; }

    public int Height { get; set; }

    public DocumentViewModelBuilder WithSize(int width, int height)
    {
        Width = width;
        Height = height;

        return this;
    }

    public abstract class StructureMemberBuilder
    {
        private MaskBuilder maskBuilder;
        
        public string Name { get; set; }
        
        public bool IsVisible { get; set; }
        
        public float Opacity { get; set; }
        
        public BlendMode BlendMode { get; set; }

        public bool HasMask => maskBuilder is not null;

        [NotNull]
        public MaskBuilder Mask => maskBuilder ??= new MaskBuilder();

        public Guid GuidValue { get; set; }

        public StructureMemberBuilder()
        {
            IsVisible = true;
            Opacity = 1;
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
        
        public StructureMemberBuilder WithGuid(Guid guid)
        {
            GuidValue = guid;
            return this;
        }
    }

    public class LayerBuilder : StructureMemberBuilder
    {
        private int? width;
        private int? height;
        
        public SurfaceBuilder Surface { get; set; }

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
        
        public new LayerBuilder WithName(string name) => base.WithName(name) as LayerBuilder;
        
        public new LayerBuilder WithVisibility(bool visibility) => base.WithVisibility(visibility) as LayerBuilder;
        
        public new LayerBuilder WithOpacity(float opacity) => base.WithOpacity(opacity) as LayerBuilder;
        
        public new LayerBuilder WithBlendMode(BlendMode blendMode) => base.WithBlendMode(blendMode) as LayerBuilder;
        
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
                throw new InvalidOperationException("You must first set the width and height of the layer. You can do this by calling WithRect() or setting the Width and Height properties.");
            }
            
            var surfaceBuilder = new SurfaceBuilder(new Surface(new VecI(Width, Height)));
            surface(surfaceBuilder);
            Surface = surfaceBuilder;
            return this;
        }
    }

    public class FolderBuilder : StructureMemberBuilder
    {
        public IEnumerable<StructureMemberBuilder> Children { get; set; }

        public new FolderBuilder WithName(string name) => base.WithName(name) as FolderBuilder;
        
        public new FolderBuilder WithVisibility(bool visibility) => base.WithVisibility(visibility) as FolderBuilder;
        
        public new FolderBuilder WithOpacity(float opacity) => base.WithOpacity(opacity) as FolderBuilder;
        
        public new FolderBuilder WithBlendMode(BlendMode blendMode) => base.WithBlendMode(blendMode) as FolderBuilder;
        
        public new FolderBuilder WithMask(Action<MaskBuilder> mask) => base.WithMask(mask) as FolderBuilder;
        
        public new FolderBuilder WithGuid(Guid guid) => base.WithGuid(guid) as FolderBuilder;
        
        public FolderBuilder WithChildren(Action<ChildrenBuilder> children)
        {
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
            Surface.SkiaSurface.Canvas.DrawBitmap(SKBitmap.Decode(buffer), 0, 0);
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
            var surfaceBuilder = new SurfaceBuilder(new Surface(new VecI(width, height)));
            surface(surfaceBuilder);
            Surface = surfaceBuilder;
            return this;
        }
    }
    
}

internal class ChildrenBuilder
{
    public ICollection<DocumentViewModelBuilder.StructureMemberBuilder> Children { get; set; } = new List<DocumentViewModelBuilder.StructureMemberBuilder>();
        
    public ChildrenBuilder WithLayer(Action<DocumentViewModelBuilder.LayerBuilder> layer)
    {
        var layerBuilder = new DocumentViewModelBuilder.LayerBuilder();
        layer(layerBuilder);
        Children.Add(layerBuilder);
        return this;
    }
    
    public ChildrenBuilder WithFolder(Action<DocumentViewModelBuilder.FolderBuilder> folder)
    {
        var folderBuilder = new DocumentViewModelBuilder.FolderBuilder();
        folder(folderBuilder);
        Children.Add(folderBuilder);
        return this;
    }
}
