using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;

namespace PixiEditor.Helpers.Behaviours;

internal class TextBlockExtensions : AvaloniaObject
{
    public static readonly AttachedProperty<IEnumerable<Inline>> BindableInlinesProperty =
        AvaloniaProperty.RegisterAttached<TextBlockExtensions, TextBlock, IEnumerable<Inline>>("BindableInlines");

    public static void SetBindableInlines(TextBlock obj, IEnumerable<Inline> value) => obj.SetValue(BindableInlinesProperty, value);
    public static IEnumerable<Inline> GetBindableInlines(TextBlock obj) => obj.GetValue(BindableInlinesProperty);

    static TextBlockExtensions()
    {
        BindableInlinesProperty.Changed.Subscribe(OnBindableInlinesChanged);
    }

    private static void OnBindableInlinesChanged(AvaloniaPropertyChangedEventArgs<IEnumerable<Inline>> e)
    {
        if (e.Sender is not TextBlock target || e.NewValue.Value is null)
        {
            return;
        }

        target.Inlines.Clear();
        target.Inlines.AddRange(e.NewValue.Value);
    }
}
