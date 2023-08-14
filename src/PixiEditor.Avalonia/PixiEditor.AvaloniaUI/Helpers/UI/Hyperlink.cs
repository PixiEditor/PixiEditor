using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using PixiEditor.OperatingSystem;

namespace PixiEditor.AvaloniaUI.Helpers.UI;

public class Hyperlink : AvaloniaObject
{
    public static AttachedProperty<string> UrlProperty
        = AvaloniaProperty.RegisterAttached<Hyperlink, TextBlock, string>(
            "Url");

    public static string GetUrl(TextBlock element)
    {
        return element.GetValue(UrlProperty);
    }

    public static void SetUrl(TextBlock element, string value)
    {
        element.SetValue(UrlProperty, value);
    }

    static Hyperlink()
    {
        UrlProperty.Changed.Subscribe(OnUrlSet);
    }

    private static void OnUrlSet(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is TextBlock tb)
        {
            tb.Classes.Add("link");
            tb.Cursor = new Cursor(StandardCursorType.Hand);
            tb.PointerPressed += (sender, args) =>
            {
                if (sender is TextBlock tb)
                {
                    if (tb.GetValue(UrlProperty) is string url)
                    {
                        IOperatingSystem.Current.OpenHyperlink(url);
                    }
                }
            };
        }
    }
}
