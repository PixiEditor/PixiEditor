using System.Collections.ObjectModel;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Brushes;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.Extensions.CommonApi.UserPreferences;
using PixiEditor.Models.BrushEngine;

namespace PixiEditor.ViewModels.BrushSystem;

internal class BrushViewModel : ViewModelBase
{
    private Texture pointPreviewTexture;
    private Texture strokeTexture;
    private Brush brush;
    private bool isFavourite;

    public Texture PointPreviewTexture
    {
        get
        {
            if (CacheChanged())
            {
                GeneratePreviewTextures();
            }

            return pointPreviewTexture;
        }
        set => SetProperty(ref pointPreviewTexture, value);
    }

    public Texture DrawingStrokeTexture
    {
        get
        {
            if (CacheChanged())
            {
                GeneratePreviewTextures();
            }

            return strokeTexture;
        }
        set => SetProperty(ref strokeTexture, value);
    }

    public string Name
    {
        get => Brush?.Name ?? "Unnamed Brush";
    }

    public ObservableCollection<string> Tags
    {
        get
        {
            if(Brush?.Tags == null)
                return new ObservableCollection<string>();

            return new ObservableCollection<string>(Brush.Tags);
        }
    }

    public Brush Brush
    {
        get { return brush; }
        set
        {
            if (SetProperty(ref brush, value))
            {
                GeneratePreviewTextures();
            }
        }
    }

    public bool IsFavourite
    {
        get => isFavourite;
        set
        {
            if (SetProperty(ref isFavourite, value))
            {
                IPreferences.Current.UpdatePreference(PreferencesConstants.FavouriteBrushes, TogglePreference());
            }
        }
    }

    private List<Guid> TogglePreference()
    {
        var current = IPreferences.Current.GetPreference<List<Guid>>(PreferencesConstants.FavouriteBrushes) ?? new List<Guid>();
        if (isFavourite)
        {
            if (!current.Contains(Brush.PersistentId))
                current.Add(Brush.PersistentId);
        }
        else
        {
            if (current.Contains(Brush.PersistentId))
                current.Remove(Brush.PersistentId);
        }

        return current;
    }

    private int lastTextureCache;

    public BrushViewModel(Brush brush)
    {
        Brush = brush;
        lastTextureCache = 0;
        isFavourite = IPreferences.Current.GetPreference<List<Guid>>(PreferencesConstants.FavouriteBrushes)?.Contains(Brush.PersistentId) ?? false;
    }

    private void GeneratePreviewTextures()
    {
        BrushOutputNode? brushNode =
            Brush?.Document?.AccessInternalReadOnlyDocument().NodeGraph.LookupNode(Brush.OutputNodeId) as BrushOutputNode;
        if (brushNode == null)
            return;

        pointPreviewTexture?.Dispose();
        strokeTexture?.Dispose();

        pointPreviewTexture =
            Texture.ForDisplay(new VecI(BrushOutputNode.PointPreviewSize, BrushOutputNode.PointPreviewSize));
        strokeTexture =
            Texture.ForDisplay(new VecI(BrushOutputNode.StrokePreviewSizeX, BrushOutputNode.StrokePreviewSizeY));

        var pointImage = new ChunkyImage(new VecI(BrushOutputNode.PointPreviewSize, BrushOutputNode.PointPreviewSize),
            ColorSpace.CreateSrgb());
        var strokeImage = new ChunkyImage(
            new VecI(BrushOutputNode.StrokePreviewSizeX, BrushOutputNode.StrokePreviewSizeY),
            ColorSpace.CreateSrgb());

        var context = new RenderContext(
            PointPreviewTexture.DrawingSurface.Canvas,
            0,
            ChunkResolution.Full,
            PointPreviewTexture.Size,
            PointPreviewTexture.Size,
            ColorSpace.CreateSrgb(),
            SamplingOptions.Bilinear,
            Brush?.Document.AccessInternalReadOnlyDocument().NodeGraph);

        brushNode.DrawPointPreview(pointImage, context,
            BrushOutputNode.PointPreviewSize,
            new VecD(BrushOutputNode.PointPreviewSize / 2,
                BrushOutputNode.PointPreviewSize / 2));

        pointImage.DrawMostUpToDateRegionOn(
            new RectI(0, 0, pointImage.CommittedSize.X, pointImage.CommittedSize.Y),
            ChunkResolution.Full,
            PointPreviewTexture.DrawingSurface.Canvas,
            VecI.Zero, null, SamplingOptions.Bilinear);

        context.RenderOutputSize = strokeTexture.Size;
        context.DocumentSize = strokeTexture.Size;
        context.RenderSurface = strokeTexture.DrawingSurface.Canvas;

        brushNode.DrawStrokePreview(strokeImage, context,
            BrushOutputNode.StrokePreviewSizeY / 2,
            new VecD(0, BrushOutputNode.YOffsetInPreview));

        strokeImage.DrawMostUpToDateRegionOn(
            new RectI(0, 0, strokeImage.CommittedSize.X, strokeImage.CommittedSize.Y),
            ChunkResolution.Full,
            strokeTexture.DrawingSurface.Canvas,
            VecI.Zero, null, SamplingOptions.Bilinear);

        OnPropertyChanged(nameof(DrawingStrokeTexture));
        OnPropertyChanged(nameof(PointPreviewTexture));
    }

    private bool CacheChanged()
    {
        var doc = Brush?.Document.AccessInternalReadOnlyDocument();
        if (doc == null)
            return false;

        int currentCache = doc.NodeGraph.GetCacheHash();
        HashCode hash = new();
        hash.Add(currentCache);
        hash.Add(doc.Blackboard.GetCacheHash());

        currentCache = hash.ToHashCode();
        if (currentCache != lastTextureCache)
        {
            lastTextureCache = currentCache;
            return true;
        }

        return false;
    }
}
