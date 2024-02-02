using Avalonia;
using Avalonia.Controls;

namespace PixiEditor.Views;

public partial class TogglableFlyout : UserControl
{
    public static readonly StyledProperty<AvaloniaObject> ChildProperty =
        AvaloniaProperty.Register<TogglableFlyout, AvaloniaObject>(nameof(Child));

    public AvaloniaObject Child
    {
        get { return GetValue(ChildProperty); }
        set { SetValue(ChildProperty, value); }
    }

    public static readonly StyledProperty<string> IconPathProperty =
        AvaloniaProperty.Register<TogglableFlyout, string>(nameof(IconPath));

    public string IconPath
    {
        get { return (string)GetValue(IconPathProperty); }
        set { SetValue(IconPathProperty, value); }
    }
    
    public TogglableFlyout()
    {
        InitializeComponent();
    }
}

