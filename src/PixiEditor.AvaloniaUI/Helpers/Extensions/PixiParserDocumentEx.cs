using ChunkyImageLib;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Numerics;
using PixiEditor.Parser;
using PixiEditor.Parser.Graph;
using PixiEditor.Parser.Skia;

namespace PixiEditor.AvaloniaUI.Helpers.Extensions;

internal static class PixiParserDocumentEx
{
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
            .WithReferenceLayer(document.ReferenceLayer, BuildReferenceLayer, document.GetEncoder())
            .WithGraph(document.Graph)
            .WithAnimationData(document.AnimationData));
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
            encoder.Decode(imgBytes, out SKColorType colorType, out SKAlphaType alphaType);
        surface.DrawBytes(surface.Size, decoded, colorType.ToColorType(), alphaType.ToAlphaType());
        
        return surface;
    }
}
