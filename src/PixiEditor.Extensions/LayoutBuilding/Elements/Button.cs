using Avalonia.Controls;
using Avalonia.Interactivity;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;
using PixiEditor.Extensions.CommonApi.LayoutBuilding.Events;

namespace PixiEditor.Extensions.LayoutBuilding.Elements;

public class Button : SingleChildLayoutElement
{
    public event ElementEventHandler Click
    {
        add => AddEvent(nameof(Click), value);
        remove => RemoveEvent(nameof(Click), value);
    }

    public Button(ILayoutElement<Control>? child = null, ElementEventHandler? onClick = null)
    {
        Child = child;
        if (onClick != null)
        {
            Click += onClick;
        }
    }

    public override Control BuildNative()
    {
        Avalonia.Controls.Button btn = new Avalonia.Controls.Button()
        {
            Content = Child?.BuildNative(),
        };

        btn.Click += (sender, args) => RaiseEvent(nameof(Click), new ElementEventArgs());

        return btn;
    }
}
