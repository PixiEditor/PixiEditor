using Avalonia.Media;

namespace PixiEditor.ViewModels.Tools.ToolSettings.Settings;

internal sealed class ColorSettingViewModel : Setting<IBrush>
{

    public IBrush BrushValue
    {
        get => base.Value;
        set
        {
            if (base.Value != null && base.Value is GradientBrush oldGradientBrush)
            {
                oldGradientBrush.GradientStops.CollectionChanged -= GradientStops_CollectionChanged;
            }

            base.Value = value;

            if (base.Value is GradientBrush gradientBrush)
            {
                gradientBrush.GradientStops.CollectionChanged += GradientStops_CollectionChanged;
            }
        }
    }

    public ColorSettingViewModel(string name, string label = "") : this(name, Brushes.White, label)
    { }
    
    public ColorSettingViewModel(string name, IBrush defaultValue, string label = "")
        : base(name)
    {
        Label = label;
        Value = defaultValue;
        ValueChanged += OnValueChanged;
    }

    private void OnValueChanged(object? sender, SettingValueChangedEventArgs<IBrush> e)
    {
        OnPropertyChanged(nameof(BrushValue));
    }

    private void GradientStops_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        InvokeValueChanged();
    }
}
