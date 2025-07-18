using Avalonia.Controls;
using Avalonia.VisualTree;
using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.Ui;
using PixiEditor.Extensions.CommonApi.Windowing;
using PixiEditor.Extensions.FlyUI.Elements;
using PixiEditor.Extensions.FlyUI.Elements.Native;
using PixiEditor.Extensions.Windowing;
using PixiEditor.Views;

namespace PixiEditor.Models.ExtensionServices;

public class VisualTreeProvider : IVisualTreeProvider
{
    ILayoutElement<T>? IVisualTreeProvider.FindElement<T>(string name)
    {
        return FindElement(name) as ILayoutElement<T>;
    }

    ILayoutElement<T>? IVisualTreeProvider.FindElement<T>(string name, IPopupWindow root)
    {
        return FindElement(name, root) as ILayoutElement<T>;
    }

    public ILayoutElement<Control>? FindElement(string name, IPopupWindow root)
    {
        var control = RecursiveLookup((root as PopupWindow).UnderlyingWindow as Window, name);
        return ToNativeType(control);
    }
    public ILayoutElement<Control>? FindElement(string name)
    {
        var control = RecursiveLookup(MainWindow.Current, name);
        return ToNativeType(control);
    }

    private static ILayoutElement<Control>? ToNativeType(Control? control)
    {
        if (control is null)
        {
            return null;
        }

        if (control is Panel panel)
        {
            NativeMultiChildElement nativeElement = new NativeMultiChildElement(panel);
            return nativeElement;
        }

        return new NativeElement(control);
    }

    private Control? RecursiveLookup(Control? control, string name)
    {
        if (control is null)
        {
            return null;
        }

        if (control.Name == name)
        {
            return control;
        }

        foreach (var child in control.GetVisualChildren())
        {
            var found = RecursiveLookup(child as Control, name);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}
