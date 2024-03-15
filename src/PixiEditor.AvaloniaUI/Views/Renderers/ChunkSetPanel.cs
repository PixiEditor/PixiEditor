using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Media;
using Avalonia.Rendering;
using Avalonia.Rendering.SceneGraph;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.AvaloniaUI.Views.Renderers;

public class ChunkSetPanel : Panel
{
    public static readonly StyledProperty<ChunkCache> ChunkCacheProperty = AvaloniaProperty.Register<ChunkSetPanel, ChunkCache>(
        nameof(ChunkCache));

    public static readonly StyledProperty<ChunkResolution> ResolutionProperty = AvaloniaProperty.Register<ChunkSetPanel, ChunkResolution>(
        nameof(Resolution));

    public static readonly StyledProperty<VecI> MaxSizeProperty = AvaloniaProperty.Register<ChunkSetPanel, VecI>(
        nameof(MaxSize));

    public VecI MaxSize
    {
        get => GetValue(MaxSizeProperty);
        set => SetValue(MaxSizeProperty, value);
    }

    public ChunkResolution Resolution
    {
        get => GetValue(ResolutionProperty);
        set => SetValue(ResolutionProperty, value);
    }

    public ChunkCache ChunkCache
    {
        get => GetValue(ChunkCacheProperty);
        set => SetValue(ChunkCacheProperty, value);
    }

    static ChunkSetPanel()
    {
        AffectsMeasure<ChunkSetPanel>(ChunkCacheProperty, ResolutionProperty, MaxSizeProperty);
    }


    protected override Size MeasureOverride(Size availableSize)
    {
        int pixelSize = Resolution.PixelSize();
        if(ChunkCache == null) return new Size(0, 0);
        Size size = new Size(pixelSize * ChunkCache[Resolution].Count, pixelSize * ChunkCache[Resolution].Count);
        if(MaxSize.X > 0 && MaxSize.Y > 0)
        {
            size = new Size(Math.Min(size.Width, MaxSize.X), Math.Min(size.Height, MaxSize.Y));
        }

        return size;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if(ChunkCache == null) return finalSize;
        int pixelSize = Resolution.PixelSize();
        double inverseMultiplier = Resolution.InvertedMultiplier();
        foreach (var child in Children)
        {
            if(child is ContentPresenter presenter && presenter.Content is KeyValuePair<VecI, Chunk> chunkPair)
            {
                if(!ChunkCache[Resolution].ContainsKey(chunkPair.Key))
                {
                    Children.Remove(presenter);
                }
                else
                {
                    double width = Math.Min(finalSize.Width, Math.Min(MaxSize.X, pixelSize)) * inverseMultiplier;
                    double height = Math.Min(finalSize.Height, Math.Min(MaxSize.Y, pixelSize)) * inverseMultiplier;
                    presenter.Arrange(new Rect(chunkPair.Key.X * pixelSize * inverseMultiplier, chunkPair.Key.Y * pixelSize * inverseMultiplier, width, height));
                }
            }
        }
        return finalSize;
    }
}
