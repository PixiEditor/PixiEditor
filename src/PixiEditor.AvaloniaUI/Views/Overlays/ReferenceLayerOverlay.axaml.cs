using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using PixiEditor.AvaloniaUI.ViewModels.Document;

namespace PixiEditor.AvaloniaUI.Views.Overlays;

[PseudoClasses(":showHighest", ":fadedOut")]
internal class ReferenceLayerOverlay : TemplatedControl
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

