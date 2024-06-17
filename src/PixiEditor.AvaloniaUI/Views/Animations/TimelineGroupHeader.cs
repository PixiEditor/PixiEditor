using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml.Templates;
using PixiEditor.AvaloniaUI.ViewModels.Document;

namespace PixiEditor.AvaloniaUI.Views.Animations;

internal class TimelineGroupHeader : TemplatedControl
{
    public static readonly StyledProperty<KeyFrameGroupViewModel> ItemProperty = AvaloniaProperty.Register<TimelineGroupHeader, KeyFrameGroupViewModel>(
        "Item");

    public KeyFrameGroupViewModel Item
    {
        get => GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }
}
