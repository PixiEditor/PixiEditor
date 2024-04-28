using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.AvaloniaUI.Views.Visuals;

namespace PixiEditor.AvaloniaUI.Views.Overlays;

[PseudoClasses(":showHighest", ":fadedOut")]
internal class ReferenceLayerOverlay : Overlay
{
    public static readonly StyledProperty<ReferenceLayerViewModel> ReferenceLayerProperty = AvaloniaProperty.Register<ReferenceLayerOverlay, ReferenceLayerViewModel>(
        nameof(ReferenceLayerViewModel));

    public static readonly StyledProperty<double> ReferenceLayerScaleProperty = AvaloniaProperty.Register<ReferenceLayerOverlay, double>(
        nameof(ReferenceLayerScale), defaultValue: 1.0f);

    public static readonly StyledProperty<bool> FadeOutProperty = AvaloniaProperty.Register<ReferenceLayerOverlay, bool>(
        nameof(FadeOut), defaultValue: false);

    public bool FadeOut
    {
        get => GetValue(FadeOutProperty);
        set => SetValue(FadeOutProperty, value);
    }

    public double ReferenceLayerScale
    {
        get => GetValue(ReferenceLayerScaleProperty);
        set => SetValue(ReferenceLayerScaleProperty, value);
    }

    public ReferenceLayerViewModel ReferenceLayer
    {
        get => GetValue(ReferenceLayerProperty);
        set => SetValue(ReferenceLayerProperty, value);
    }

    static ReferenceLayerOverlay()
    {
        ReferenceLayerProperty.Changed.Subscribe(ReferenceLayerChanged);
        FadeOutProperty.Changed.Subscribe(FadeOutChanged);
    }

    public override void Render(DrawingContext context)
    {
        if (ReferenceLayer != null && ReferenceLayer.ReferenceBitmap != null)
        {
            Rect dirtyRect = new Rect(CanvasDirtyBounds.X, CanvasDirtyBounds.Y, CanvasDirtyBounds.Width, CanvasDirtyBounds.Height);
            DrawSurfaceOperation drawOperation = new DrawSurfaceOperation(dirtyRect, ReferenceLayer.ReferenceBitmap, Stretch.Uniform, Opacity);
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

