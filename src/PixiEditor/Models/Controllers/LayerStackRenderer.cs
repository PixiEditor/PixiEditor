using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Layers.Utils;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixiEditor.Models.Controllers;

public class LayerStackRenderer : INotifyPropertyChanged, IDisposable
{
    private SKPaint BlendingPaint { get; } = new SKPaint() { BlendMode = SKBlendMode.SrcOver };
    private SKPaint ClearPaint { get; } = new SKPaint() { BlendMode = SKBlendMode.Src, Color = SKColors.Transparent };

    private System.Collections.ObjectModel.ObservableCollection<Layer> layers;
    private LayerStructure structure;

    private Surface finalSurface;
    private SKSurface backingSurface;
    private WriteableBitmap finalBitmap;
    public WriteableBitmap FinalBitmap
    {
        get => finalBitmap;
        set
        {
            finalBitmap = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FinalBitmap)));
        }
    }

    public Surface FinalSurface { get => finalSurface; }

    public event PropertyChangedEventHandler PropertyChanged;
    public LayerStackRenderer(System.Collections.ObjectModel.ObservableCollection<Layer> layers, LayerStructure structure, int width, int height)
    {
        this.layers = layers;
        this.structure = structure;
        layers.CollectionChanged += OnLayersChanged;
        SubscribeToAllLayers(layers);
        Resize(width, height);
    }

    public void Resize(int newWidth, int newHeight)
    {
        finalSurface?.Dispose();
        backingSurface?.Dispose();
        finalSurface = new Surface(newWidth, newHeight);
        FinalBitmap = new WriteableBitmap(newWidth, newHeight, 96, 96, PixelFormats.Pbgra32, null);
        var imageInfo = new SKImageInfo(newWidth, newHeight, SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb());
        backingSurface = SKSurface.Create(imageInfo, finalBitmap.BackBuffer, finalBitmap.BackBufferStride);
        Update(new Int32Rect(0, 0, newWidth, newHeight));
    }

    public void SetNewLayersCollection(System.Collections.ObjectModel.ObservableCollection<Layer> layers)
    {
        layers.CollectionChanged -= OnLayersChanged;
        UnsubscribeFromAllLayers(this.layers);
        this.layers = layers;
        SubscribeToAllLayers(layers);
        layers.CollectionChanged += OnLayersChanged;
        Update(new Int32Rect(0, 0, finalSurface.Width, finalSurface.Height));
    }

    public void ForceRerender()
    {
        Update(new Int32Rect(0, 0, finalSurface.Width, finalSurface.Height));
    }

    public void Dispose()
    {
        finalSurface.Dispose();
        backingSurface.Dispose();
        BlendingPaint.Dispose();
        ClearPaint.Dispose();
        layers.CollectionChanged -= OnLayersChanged;
    }

    private void SubscribeToAllLayers(System.Collections.ObjectModel.ObservableCollection<Layer> layers)
    {
        foreach (var layer in layers)
        {
            layer.LayerBitmapChanged += OnLayerBitmapChanged;
        }
    }

    private void UnsubscribeFromAllLayers(System.Collections.ObjectModel.ObservableCollection<Layer> layers)
    {
        foreach (var layer in layers)
        {
            layer.LayerBitmapChanged -= OnLayerBitmapChanged;
        }
    }

    private void Update(Int32Rect dirtyRectangle)
    {
        dirtyRectangle = dirtyRectangle.Intersect(new Int32Rect(0, 0, finalBitmap.PixelWidth, finalBitmap.PixelHeight));
        finalSurface.SkiaSurface.Canvas.DrawRect(
            new SKRect(
                dirtyRectangle.X, dirtyRectangle.Y,
                dirtyRectangle.X + dirtyRectangle.Width,
                dirtyRectangle.Y + dirtyRectangle.Height
            ),
            ClearPaint
        );
        foreach (var layer in layers)
        {
            if (!LayerStructureUtils.GetFinalLayerIsVisible(layer, structure))
                continue;
            BlendingPaint.Color = new SKColor(255, 255, 255, (byte)(LayerStructureUtils.GetFinalLayerOpacity(layer, structure) * 255));

            Int32Rect layerRect = new Int32Rect(layer.OffsetX, layer.OffsetY, layer.Width, layer.Height);
            Int32Rect layerPortion = layerRect.Intersect(dirtyRectangle);

            using var snapshot = layer.LayerBitmap.SkiaSurface.Snapshot();
            finalSurface.SkiaSurface.Canvas.DrawImage(
                snapshot,
                new SKRect(
                    layerPortion.X - layer.OffsetX,
                    layerPortion.Y - layer.OffsetY,
                    layerPortion.X - layer.OffsetX + layerPortion.Width,
                    layerPortion.Y - layer.OffsetY + layerPortion.Height),
                new SKRect(
                    layerPortion.X,
                    layerPortion.Y,
                    layerPortion.X + layerPortion.Width,
                    layerPortion.Y + layerPortion.Height
                ),
                BlendingPaint);
        }
        finalBitmap.Lock();
        using (var snapshot = finalSurface.SkiaSurface.Snapshot())
        {
            SKRect rect = new(dirtyRectangle.X, dirtyRectangle.Y, dirtyRectangle.X + dirtyRectangle.Width, dirtyRectangle.Y + dirtyRectangle.Height);
            backingSurface.Canvas.DrawImage(snapshot, rect, rect, Surface.ReplacingPaint);
        }

        finalBitmap.AddDirtyRect(dirtyRectangle);
        finalBitmap.Unlock();
    }

    private void OnLayersChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (var obj in e.NewItems)
            {
                Layer layer = (Layer)obj;
                layer.LayerBitmapChanged += OnLayerBitmapChanged;
            }
        }
        if (e.OldItems != null)
        {
            foreach (var obj in e.OldItems)
            {
                ((Layer)obj).LayerBitmapChanged -= OnLayerBitmapChanged;
            }
        }
        Update(new Int32Rect(0, 0, finalSurface.Width, finalSurface.Height));
    }

    private void OnLayerBitmapChanged(object sender, Int32Rect e)
    {
        Update(e);
    }
}