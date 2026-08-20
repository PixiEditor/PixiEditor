using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;

namespace PixiEditor.UI.Common.Controls;

public class CaptionButtonsPlaceholder : TemplatedControl
{
    public static readonly StyledProperty<bool> CanMinimizeProperty = AvaloniaProperty.Register<CaptionButtonsPlaceholder, bool>(
        nameof(CanMinimize), true);

    public static readonly StyledProperty<bool> CanMaximizeProperty = AvaloniaProperty.Register<CaptionButtonsPlaceholder, bool>(
        nameof(CanMaximize), true);

    public bool CanMaximize
    {
        get => GetValue(CanMaximizeProperty);
        set => SetValue(CanMaximizeProperty, value);
    }

    public bool CanMinimize
    {
        get => GetValue(CanMinimizeProperty);
        set => SetValue(CanMinimizeProperty, value);
    }
}

