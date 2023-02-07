using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.Models.DataHolders;
using PixiEditor.Parser;
using PixiEditor.Parser.Collections;
using BlendMode = PixiEditor.Parser.BlendMode;
using PixiDocument = PixiEditor.Parser.Document;
using PixiColor = PixiEditor.DrawingApi.Core.ColorsImpl.Color;

namespace PixiEditor.ViewModels.SubViewModels.Document;

internal partial class DocumentViewModel
{
    public PixiDocument ToSerializable()
    {
        var root = new Folder();
        
        var doc = Internals.Tracker.Document;

        AddMembers(doc.StructureRoot.Children, doc, root);

        var document = new PixiDocument
        {
            Width = Width, Height = Height,
            Swatches = ToCollection(Swatches), Palette = ToCollection(Palette),
            RootFolder = root, PreviewImage = (MaybeRenderWholeImage().Value as Surface)?.DrawingSurface.Snapshot().Encode().AsSpan().ToArray()
        };

        return document;
    }

    private static void AddMembers(IEnumerable<IReadOnlyStructureMember> members, IReadOnlyDocument document, Folder parent)
    {
        foreach (var member in members)
        {
            if (member is IReadOnlyFolder readOnlyFolder)
            {
                var folder = ToSerializable(readOnlyFolder);

                AddMembers(readOnlyFolder.Children, document, folder);

                parent.Children.Add(folder);
            }
            else if (member is IReadOnlyLayer readOnlyLayer)
            {
                parent.Children.Add(ToSerializable(readOnlyLayer, document));
            }
        }
    }
    
    private static Folder ToSerializable(IReadOnlyFolder folder)
    {
        return new Folder
        {
            Name = folder.Name,
            BlendMode = (BlendMode)(int)folder.BlendMode,
            Enabled = folder.IsVisible,
            Opacity = folder.Opacity,
            ClipToMemberBelow = folder.ClipToMemberBelow,
            Mask = GetMask(folder.Mask, folder.MaskIsVisible)
        };
    }
    
    private static ImageLayer ToSerializable(IReadOnlyLayer layer, IReadOnlyDocument document)
    {
        var result = document.GetLayerImage(layer.GuidValue);

        var tightBounds = document.GetLayerTightBounds(layer.GuidValue);
        using var data = result?.DrawingSurface.Snapshot().Encode();
        byte[] bytes = data?.AsSpan().ToArray();
        var serializable = new ImageLayer
        {
            Width = result?.Size.X ?? 0, Height = result?.Size.Y ?? 0, OffsetX = tightBounds?.X ?? 0, OffsetY = tightBounds?.Y ?? 0,
            Enabled = layer.IsVisible, BlendMode = (BlendMode)(int)layer.BlendMode, ImageBytes = bytes,
            ClipToMemberBelow = layer.ClipToMemberBelow, Name = layer.Name, 
            Opacity = layer.Opacity, Mask = GetMask(layer.Mask, layer.MaskIsVisible)
        };

        return serializable;
    }

    private static Mask GetMask(IReadOnlyChunkyImage mask, bool maskVisible)
    {
        if (mask == null) 
            return null;
        
        var maskBound = mask.FindLatestBounds();

        if (maskBound == null)
        {
            return new Mask();
        }
        
        var surface = DrawingBackendApi.Current.SurfaceImplementation.Create(new ImageInfo(
            maskBound.Value.Width,
            maskBound.Value.Height));
                
        mask.DrawMostUpToDateRegionOn(new RectI(0, 0, maskBound.Value.Width, maskBound.Value.Height), ChunkResolution.Full, surface, new VecI(0, 0));

        return new Mask
        {
            Width = maskBound.Value.Width, Height = maskBound.Value.Height,
            OffsetX = maskBound.Value.X, OffsetY = maskBound.Value.Y,
            Enabled = maskVisible, ImageBytes = surface.Snapshot().Encode().AsSpan().ToArray()
        };
    }

    private ColorCollection ToCollection(WpfObservableRangeCollection<PixiColor> collection) =>
        new(collection.Select(x => Color.FromArgb(x.A, x.R, x.G, x.B)));
}
