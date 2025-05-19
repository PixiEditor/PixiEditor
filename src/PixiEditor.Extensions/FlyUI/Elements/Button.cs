using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using PixiEditor.Extensions.CommonApi.FlyUI.Events;

namespace PixiEditor.Extensions.FlyUI.Elements;

public class Button : SingleChildLayoutElement
{
    private Avalonia.Controls.Button _button;
    public event ElementEventHandler Click
    {
        add => AddEvent(nameof(Click), value);
        remove => RemoveEvent(nameof(Click), value);
    }

    public Button()
    {

    }

    public Button(LayoutElement? child = null, ElementEventHandler? onClick = null)
    {
        Child = child;
        if (onClick != null)
        {
            Click += onClick;
        }
    }

    protected override Control CreateNativeControl()
    {
        _button = new Avalonia.Controls.Button();
        Binding binding = new Binding(nameof(Child)) { Source = this, Converter = LayoutElementToNativeControlConverter.Instance };
        _button.Bind(Avalonia.Controls.ContentControl.ContentProperty, binding);

        _button.Click += (sender, args) => RaiseEvent(nameof(Click), new ElementEventArgs() { Sender = this });

        return _button;
    }

    protected override void AddChild(Control child)
    {
        _button.Content = child;
    }

    protected override void RemoveChild()
    {
        _button.Content = null;
    }
}
