using Avalonia.Controls;
using PixiEditor.AvaloniaUI.Helpers.Behaviours;

namespace PixiEditor.AvaloniaUI.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        TextBoxFocusBehavior.FallbackFocusElement = FocusableGrid;
    }
}
