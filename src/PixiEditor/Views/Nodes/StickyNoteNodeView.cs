using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;

namespace PixiEditor.Views.Nodes;

public class StickyNoteNodeView : NodeView
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<StickyNoteNodeView, string>(
            nameof(Title), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<StickyNoteNodeView, string>(
            nameof(Text), defaultBindingMode: BindingMode.TwoWay);

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (e.Handled) return;
        base.OnPointerPressed(e);
    }
}
