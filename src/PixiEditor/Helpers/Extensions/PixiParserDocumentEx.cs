using ChunkyImageLib;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Extensions.Palettes;
using PixiEditor.Numerics;
using PixiEditor.Parser;
using PixiEditor.Parser.Deprecated;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.Helpers.Extensions;

internal static class PixiParserDocumentEx
{
    public static VecD ToVecD(this Vector2 vec)
    {
        return new VecD(vec.X, vec.Y);
    }
    
    public static DocumentViewModel ToDocument(this Document document)
    {
        return DocumentViewModel.Build(b =>
        {
            b.WithSize(document.Width, document.Height)
                .WithPalette(document.Palette, x => new PaletteColor(x.R, x.G, x.B))
                .WithSwatches(document.Swatches, x => new(x.R, x.G, x.B))
                .WithReferenceLayer(document.ReferenceLayer, (r, builder) => builder
                    .WithIsVisible(r.Enabled)
                    .WithShape(r.Corners)
                    .WithSurface(Surface.Load(r.ImageBytes)));

            BuildChildren(b, document.RootFolder.Children);
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
                    throw new NotImplementedException($"StructureMember of type '{member.GetType().FullName}' has not been implemented");
                }
            }
        }

        void BuildFolder(DocumentViewModelBuilder.FolderBuilder builder, Folder folder) => builder
            .WithName(folder.Name)
            .WithVisibility(folder.Enabled)
            .WithOpacity(folder.Opacity)
            .WithBlendMode((PixiEditor.ChangeableDocument.Enums.BlendMode)(int)folder.BlendMode)
            .WithChildren(x => BuildChildren(x, folder.Children))
            .WithClipToBelow(folder.ClipToMemberBelow)
            .WithMask(folder.Mask, (x, m) => x.WithVisibility(m.Enabled).WithSurface(m.Width, m.Height, x => x.WithImage(m.ImageBytes, m.OffsetX, m.OffsetY)));

        void BuildLayer(DocumentViewModelBuilder.LayerBuilder builder, ImageLayer layer)
        {
            builder
                .WithName(layer.Name)
                .WithVisibility(layer.Enabled)
                .WithOpacity(layer.Opacity)
                .WithBlendMode((PixiEditor.ChangeableDocument.Enums.BlendMode)(int)layer.BlendMode)
                .WithRect(layer.Width, layer.Height, layer.OffsetX, layer.OffsetY)
                .WithClipToBelow(layer.ClipToMemberBelow)
                .WithLockAlpha(layer.LockAlpha)
                .WithMask(layer.Mask,
                    (x, m) => x.WithVisibility(m.Enabled).WithSurface(m.Width, m.Height,
                        x => x.WithImage(m.ImageBytes, m.OffsetX, m.OffsetY)));

            if (layer.Width > 0 && layer.Height > 0)
            {
                builder.WithSurface(x => x.WithImage(layer.ImageBytes, 0, 0));
            }
        }
    }
    
    public static SKBitmap RenderOldDocument(this SerializableDocument document)
    {
        SKImageInfo info = new(document.Width, document.Height, SKColorType.RgbaF32, SKAlphaType.Unpremul, SKColorSpace.CreateSrgb());
        using SKSurface surface = SKSurface.Create(info);
        SKCanvas canvas = surface.Canvas;
        using SKPaint paint = new();

        foreach (var layer in document)
        {
            if (layer.PngBytes == null || layer.PngBytes.Length == 0)
            {
                continue;
            }

            bool visible = document.Layers.GetFinalLayerVisibilty(layer);

            if (!visible)
            {
                continue;
            }

            double opacity = document.Layers.GetFinalLayerOpacity(layer);

            if (opacity == 0)
            {
                continue;
            }

            using SKColorFilter filter = SKColorFilter.CreateBlendMode(SKColors.White.WithAlpha((byte)(opacity * 255)), SKBlendMode.DstIn);
            paint.ColorFilter = filter;

            using var image = SKImage.FromEncodedData(layer.PngBytes);
            
            canvas.DrawImage(image, layer.OffsetX, layer.OffsetY, paint);
        }

        SKBitmap bitmap = new(info);

        surface.ReadPixels(info, bitmap.GetPixels(), info.RowBytes, 0, 0);

        return bitmap;
    }
}
