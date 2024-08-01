using Avalonia;
using Avalonia.Controls;

namespace PixiEditor.Views.Overlays;

public partial class TogglableFlyout : UserControl
{
    public static readonly StyledProperty<AvaloniaObject> ChildProperty =
        AvaloniaProperty.Register<TogglableFlyout, AvaloniaObject>(nameof(Child));

    public AvaloniaObject Child
    {
        get { return GetValue(ChildProperty); }
        set { SetValue(ChildProperty, value); }
    }

    public static readonly StyledProperty<string> IconProperty =
        AvaloniaProperty.Register<TogglableFlyout, string>(nameof(Icon));

    public string Icon
    {
        get { return (string)GetValue(IconProperty); }
        set { SetValue(IconProperty, value); }
    }
    
    public TogglableFlyout()
    {
        InitializeComponent();
    }
}

