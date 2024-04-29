using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using ChunkyImageLib.DataHolders;
using PixiEditor.AvaloniaUI.Helpers.Converters;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.AvaloniaUI.Views.Visuals;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.AvaloniaUI.Views.Overlays;

internal class ReferenceLayerOverlay : Overlay
{
    public static readonly StyledProperty<ReferenceLayerViewModel> ReferenceLayerProperty = AvaloniaProperty.Register<ReferenceLayerOverlay, ReferenceLayerViewModel>(
        nameof(ReferenceLayerViewModel));

    public static readonly StyledProperty<bool> FadeOutProperty = AvaloniaProperty.Register<ReferenceLayerOverlay, bool>(
        nameof(FadeOut), defaultValue: false);

    public static readonly StyledProperty<ShapeCorners> ReferenceShapeProperty = AvaloniaProperty.Register<ReferenceLayerOverlay, ShapeCorners>(
        nameof(ReferenceShape));

    public ShapeCorners ReferenceShape
    {
        get => GetValue(ReferenceShapeProperty);
        set => SetValue(ReferenceShapeProperty, value);
    }

    public ReferenceLayerViewModel ReferenceLayer
    {
        get => GetValue(ReferenceLayerProperty);
        set => SetValue(ReferenceLayerProperty, value);
    }


    public bool FadeOut
    {
        get => GetValue(FadeOutProperty);
        set => SetValue(FadeOutProperty, value);
    }

    public double ReferenceLayerScale => ((ReferenceLayer.ReferenceBitmap != null && ReferenceShape != null)
        ? (ReferenceShape.RectSize.X / (double)ReferenceLayer.ReferenceBitmap.Size.X)
        : 1);

    public override OverlayRenderSorting OverlayRenderSorting => ReferenceLayer.IsTopMost ? OverlayRenderSorting.Foreground : OverlayRenderSorting.Background;

    static ReferenceLayerOverlay()
    {
        ReferenceLayerProperty.Changed.Subscribe(ReferenceLayerChanged);
        FadeOutProperty.Changed.Subscribe(FadeOutChanged);
    }

    public override void RenderOverlay(DrawingContext context, RectD dirtyCanvasBounds)
    {
        //TODO: opacity + animation + border
        if (ReferenceLayer is { ReferenceBitmap: not null })
        {
            using var renderOptions = context.PushRenderOptions(new RenderOptions { BitmapInterpolationMode = ScaleToBitmapScalingModeConverter.Calculate(ReferenceLayerScale) });
            using var matrix = context.PushTransform(ReferenceLayer.ReferenceTransformMatrix);

            Rect dirtyRect = new Rect(dirtyCanvasBounds.X, dirtyCanvasBounds.Y, dirtyCanvasBounds.Width, dirtyCanvasBounds.Height);
            DrawSurfaceOperation drawOperation = new DrawSurfaceOperation(dirtyRect, ReferenceLayer.ReferenceBitmap, Stretch.None, Opacity);
            context.Custom(drawOperation);
        }
    }

    private static void ReferenceLayerChanged(AvaloniaPropertyChangedEventArgs<ReferenceLayerViewModel> obj)
    {
        ReferenceLayerOverlay objSender = (ReferenceLayerOverlay)obj.Sender;
        if (obj.OldValue.Value != null)
        {
            obj.OldValue.Value.PropertyChanged -= objSender.ReferenceLayerOnPropertyChanged;
        }

        if (obj.NewValue.Value != null)
        {
            obj.NewValue.Value.PropertyChanged += objSender.ReferenceLayerOnPropertyChanged;
        }
    }

    private static void FadeOutChanged(AvaloniaPropertyChangedEventArgs<bool> obj)
    {
        ReferenceLayerOverlay objSender = (ReferenceLayerOverlay)obj.Sender;
        objSender.PseudoClasses.Set(":fadedOut", obj.NewValue.Value);
    }

    private void ReferenceLayerOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        PseudoClasses.Set(":showHighest", ReferenceLayer.ShowHighest);
    }
}

