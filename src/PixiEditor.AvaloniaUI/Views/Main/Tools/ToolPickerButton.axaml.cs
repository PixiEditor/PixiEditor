using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace PixiEditor.AvaloniaUI.Views.Main.Tools;

[PseudoClasses(":selected")]
public partial class ToolPickerButton : UserControl
{
    public static readonly StyledProperty<bool> IsSelectedProperty = AvaloniaProperty.Register<ToolPickerButton, bool>(
        nameof(IsSelected));

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }
    public ToolPickerButton()
    {
        IObservable<bool> isSelectedObservable = this.GetObservable(IsSelectedProperty);
        PseudoClasses.Set(":selected", isSelectedObservable);
        InitializeComponent();
    }
}

