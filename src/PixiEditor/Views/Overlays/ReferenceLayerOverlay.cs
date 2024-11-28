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
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using PixiEditor.Helpers.Converters;
using Drawie.Numerics;
using PixiEditor.ViewModels.Document;
using PixiEditor.Views.Visuals;
using Canvas = Drawie.Backend.Core.Surfaces.Canvas;
using Color = Drawie.Backend.Core.ColorsImpl.Color;
using Colors = Drawie.Backend.Core.ColorsImpl.Colors;

namespace PixiEditor.Views.Overlays;

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

    public double ReferenceLayerScale => ((ReferenceLayer.ReferenceTexture != null && ReferenceShape != null)
        ? (ReferenceShape.RectSize.X / (double)ReferenceLayer.ReferenceTexture.Size.X)
        : 1);

    public override OverlayRenderSorting OverlayRenderSorting => ReferenceLayer.IsTopMost
        ? OverlayRenderSorting.Foreground
        : OverlayRenderSorting.Background;

    private Paint borderBen = new Paint() { Color = Colors.Black, StrokeWidth = 2, Style = PaintStyle.Stroke };
    
    private Paint overlayPaint = new Paint
    {
        Color = new Color(255, 255, 255, 255),
        BlendMode = BlendMode.SrcOver
    };

    static ReferenceLayerOverlay()
    {
        ReferenceLayerProperty.Changed.Subscribe(ReferenceLayerChanged);
        FadeOutProperty.Changed.Subscribe(FadeOutChanged);
    }

    public override void RenderOverlay(Canvas context, RectD dirtyCanvasBounds)
    {
        if (ReferenceLayer is { ReferenceTexture: not null })
        {
            var interpolationMode = ScaleToBitmapScalingModeConverter.Calculate(ReferenceLayerScale);

            int saved = context.Save();
            
            context.SetMatrix(context.TotalMatrix.Concat(ReferenceLayer.ReferenceTransformMatrix));

            RectD dirty = new RectD(0, 0, ReferenceLayer.ReferenceTexture.Size.X, ReferenceLayer.ReferenceTexture.Size.Y);
            Rect dirtyRect = new Rect(dirty.X, dirty.Y, dirty.Width, dirty.Height);

            double opacity = Opacity;
            var referenceBitmap = ReferenceLayer.ReferenceTexture;

            overlayPaint.Color = new Color(255, 255, 255, (byte)(opacity * 255));

            context.DrawSurface(referenceBitmap.DrawingSurface, 0, 0, overlayPaint); 
            
            context.RestoreToCount(saved);
            
            DrawBorder(context, dirtyCanvasBounds);
        }
    }

    private void DrawBorder(Canvas context, RectD dirtyCanvasBounds)
    {
        context.DrawRect(new RectD(0, 0, dirtyCanvasBounds.Width, dirtyCanvasBounds.Height), borderBen);
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

    protected override void ZoomChanged(double newZoom)
    {
        borderBen.StrokeWidth = 2 / (float)newZoom;
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

    private static void FadeOutChanged(AvaloniaPropertyChangedEventArgs<bool> obj)
    {
        ReferenceLayerOverlay objSender = (ReferenceLayerOverlay)obj.Sender;
        objSender.ToggleFadeOut(obj.NewValue.Value);
    }
}
