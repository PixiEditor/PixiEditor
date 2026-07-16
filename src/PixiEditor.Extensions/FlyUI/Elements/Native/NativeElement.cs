using Avalonia.Controls;
using Avalonia.Input;
using PixiEditor.Extensions.CommonApi.FlyUI.Events;

namespace PixiEditor.Extensions.FlyUI.Elements.Native;

public class NativeElement : LayoutElement
{
    public Control Native { get; }

    public NativeElement(Control native)
    {
        Native = native;
    }

    protected override Control CreateNativeControl()
    {
        return Native;
    }

    protected override void OnAddEvent(string eventName)
    {
        // TODO: Not the best solution, think of something better
        if(eventName == "Click" && Native is Avalonia.Controls.Button button)
        {
            button.Click += (s, e) => RaiseEvent("Click", new ElementEventArgs() {Sender = this });
        }
    }
}
