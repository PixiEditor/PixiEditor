using System.ComponentModel;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using ChunkyImageLib.DataHolders;
using PixiEditor.AvaloniaUI.Helpers.Converters;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.AvaloniaUI.Views.Visuals;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.AvaloniaUI.Views.Overlays;

internal class ReferenceLayerOverlay : Overlay
{
    private const float OpacityTransitionDuration = 0.1f;
    public static readonly StyledProperty<ReferenceLayerViewModel> ReferenceLayerProperty =
        AvaloniaProperty.Register<ReferenceLayerOverlay, ReferenceLayerViewModel>(
            nameof(ReferenceLayerViewModel));

    public static readonly StyledProperty<bool> FadeOutProperty =
        AvaloniaProperty.Register<ReferenceLayerOverlay, bool>(
            nameof(FadeOut), defaultValue: false);

    public static readonly StyledProperty<ShapeCorners> ReferenceShapeProperty =
        AvaloniaProperty.Register<ReferenceLayerOverlay, ShapeCorners>(
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

    public override OverlayRenderSorting OverlayRenderSorting => ReferenceLayer.IsTopMost
        ? OverlayRenderSorting.Foreground
        : OverlayRenderSorting.Background;

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
            using var renderOptions = context.PushRenderOptions(new RenderOptions
            {
                BitmapInterpolationMode = ScaleToBitmapScalingModeConverter.Calculate(ReferenceLayerScale)
            });
            using var matrix = context.PushTransform(ReferenceLayer.ReferenceTransformMatrix);

            RectD dirty = new RectD(0, 0, ReferenceLayer.ReferenceBitmap.Size.X, ReferenceLayer.ReferenceBitmap.Size.Y);
            Rect dirtyRect = new Rect(dirty.X, dirty.Y, dirty.Width, dirty.Height);
            DrawSurfaceOperation drawOperation =
                new DrawSurfaceOperation(dirtyRect, ReferenceLayer.ReferenceBitmap, Stretch.None, Opacity);
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
        objSender.ToggleFadeOut(obj.NewValue.Value);
    }

    private void ToggleFadeOut(bool toggle)
    {
        double targetOpaqueOpacity = ReferenceLayer.ShowHighest ? ReferenceLayerViewModel.TopMostOpacity : 1;
        TransitionTo(OpacityProperty, OpacityTransitionDuration, toggle ? 0 : targetOpaqueOpacity);
    }

    private void ReferenceLayerOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        double targetOpaqueOpacity = ReferenceLayer.ShowHighest ? ReferenceLayerViewModel.TopMostOpacity : 1;
        TransitionTo(OpacityProperty, OpacityTransitionDuration, FadeOut ? 0 : targetOpaqueOpacity);

    }
}
