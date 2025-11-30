using Avalonia.Media;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using PixiEditor.Helpers.Extensions;
using Colors = Drawie.Backend.Core.ColorsImpl.Colors;

namespace PixiEditor.ViewModels.Tools.ToolSettings.Settings;

internal sealed class PaintableSettingViewModel : Setting<Paintable>
{
    private IBrush brushValue;
    public IBrush BrushValue
    {
        get => base.Value.ToBrush();
        set
        {
            if (brushValue != null && brushValue is GradientBrush oldGradientBrush)
            {
                oldGradientBrush.GradientStops.CollectionChanged -= GradientStops_CollectionChanged;
            }

            brushValue = value;
            base.Value = value.ToPaintable();

            if (brushValue is GradientBrush gradientBrush)
            {
                gradientBrush.GradientStops.CollectionChanged += GradientStops_CollectionChanged;
            }
        }
    }


    public PaintableSettingViewModel(string name, string label = "") : this(name, new ColorPaintable(Colors.White), label)
    { }
    
    public PaintableSettingViewModel(string name, Paintable defaultValue, string label = "")
        : base(name)
    {
        Label = label;
        Value = defaultValue;
        ValueChanged += OnValueChanged;
    }

    private void OnValueChanged(object? sender, SettingValueChangedEventArgs<Paintable> e)
    {
        OnPropertyChanged(nameof(BrushValue));
    }

    private void GradientStops_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        InvokeValueChanged();
    }
}
