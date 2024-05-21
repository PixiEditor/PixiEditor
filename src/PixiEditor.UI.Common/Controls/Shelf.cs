using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Threading;

namespace PixiEditor.UI.Common.Controls;

[PseudoClasses(":isOpen")]
public class Shelf : ContentControl
{
    public static readonly StyledProperty<bool> IsOpenProperty = AvaloniaProperty.Register<Shelf, bool>(
        nameof(IsOpen), true);

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    static Shelf()
    {
        IsOpenProperty.Changed.Subscribe(OnIsOpenChanged);
    }

    public Shelf()
    {
        PseudoClasses.Set(":isOpen", true);
    }

    private static void OnIsOpenChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var shelf = (Shelf)e.Sender;
        shelf.PseudoClasses.Set(":isOpen", (bool)e.NewValue);
    }
}
