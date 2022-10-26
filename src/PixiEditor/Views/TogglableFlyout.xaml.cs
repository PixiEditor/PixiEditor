using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace PixiEditor.Views;

public partial class TogglableFlyout : UserControl
{
    public static readonly DependencyProperty ChildProperty = DependencyProperty.Register(
        nameof(Child), typeof(UIElement), typeof(TogglableFlyout), new PropertyMetadata(default(UIElement)));

    [Bindable(true)]
    [Category("Content")]
    public UIElement Child
    {
        get { return (UIElement)GetValue(ChildProperty); }
        set { SetValue(ChildProperty, value); }
    }

    public static readonly DependencyProperty IconPathProperty = DependencyProperty.Register(
        nameof(IconPath), typeof(string), typeof(TogglableFlyout), new PropertyMetadata(default(string)));

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

