using Avalonia;
using Avalonia.Controls;
using PixiEditor.Models.Input;

namespace PixiEditor.Views.Shortcuts;

public partial class ShortcutHint : UserControl
{
    public static readonly StyledProperty<KeyCombination> ShortcutProperty =
        AvaloniaProperty.Register<ShortcutHint, KeyCombination>(nameof(Shortcut));

    public KeyCombination Shortcut
    {
        get => GetValue(ShortcutProperty);
        set => SetValue(ShortcutProperty, value);
    }
    
    public ShortcutHint()
    {
        var shortcutObserver = this.GetObservable(ShortcutProperty);
        shortcutObserver.Subscribe(_ => UpdateVisibility());
        
        InitializeComponent();
        
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        IsVisible = Shortcut != KeyCombination.None;
    }
}

