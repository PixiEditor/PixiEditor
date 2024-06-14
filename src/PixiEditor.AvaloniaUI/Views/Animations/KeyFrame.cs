using Avalonia;
using Avalonia.Controls.Primitives;
using PixiEditor.AvaloniaUI.ViewModels.Document;

namespace PixiEditor.AvaloniaUI.Views.Animations;

public class KeyFrame : TemplatedControl
{
    public static readonly StyledProperty<KeyFrameViewModel> ItemProperty = AvaloniaProperty.Register<KeyFrame, KeyFrameViewModel>(
        nameof(Item));

    public KeyFrameViewModel Item
    {
        get => GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }
}
