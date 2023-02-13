using ChunkyImageLib;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.IO;
using PixiEditor.Parser;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.Helpers.Extensions;

internal static class PixiParserDocumentEx
{
    public static DocumentViewModel ToDocument(this Document document)
    {
        return DocumentViewModel.Build(b =>
        {
            b.WithSize(document.Width, document.Height)
                .WithPalette(document.Palette, x => new Color(x.R, x.G, x.B, x.A))
                .WithSwatches(document.Swatches, x => new(x.R, x.G, x.B, x.A))
                .WithReferenceLayer(document.ReferenceLayer, (r, builder) => builder
                    .WithIsVisible(r.Enabled)
                    .WithRect(new VecD(r.OffsetX, r.OffsetY), new VecD(r.Width, r.Height))
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
                .WithMask(layer.Mask,
                    (x, m) => x.WithVisibility(m.Enabled).WithSurface(m.Width, m.Height,
                        x => x.WithImage(m.ImageBytes, m.OffsetX, m.OffsetY)));

            if (layer.Width > 0 && layer.Height > 0)
            {
                builder.WithSurface(x => x.WithImage(layer.ImageBytes, 0, 0));
            }
        }
    }
}
