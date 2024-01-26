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

    public Button(ILayoutElement<Control>? child = null)
    {
        Child = child;
    }

    public override Control Build()
    {
        Avalonia.Controls.Button btn = new Avalonia.Controls.Button()
        {
            Content = Child?.Build(),
        };

        btn.Click += (sender, args) => RaiseEvent(nameof(Click), new ElementEventArgs());

        return btn;
    }
}
