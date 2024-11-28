using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using PixiEditor.OperatingSystem;

namespace PixiEditor.Extensions.UI;

public class Hyperlink : AvaloniaObject
{
    public static AttachedProperty<string> UrlProperty
        = AvaloniaProperty.RegisterAttached<Hyperlink, TextBlock, string>(
            "Url");

    public static readonly AttachedProperty<ICommand> CommandProperty =
        AvaloniaProperty.RegisterAttached<Hyperlink, TextBlock, ICommand>("Command");

    public static readonly AttachedProperty<object> CommandParameterProperty =
        AvaloniaProperty.RegisterAttached<Hyperlink, TextBlock, object>("CommandParameter");

    public static void SetCommandParameter(TextBlock obj, object value) => obj.SetValue(CommandParameterProperty, value);
    public static object GetCommandParameter(TextBlock obj) => obj.GetValue(CommandParameterProperty);

    public static void SetCommand(TextBlock obj, ICommand value) => obj.SetValue(CommandProperty, value);
    public static ICommand GetCommand(TextBlock obj) => obj.GetValue(CommandProperty);

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
        UrlProperty.Changed.AddClassHandler<TextBlock>(OnUrlSet);
        CommandProperty.Changed.AddClassHandler<TextBlock>(OnCommandSet);
    }

    private static void OnUrlSet(TextBlock textBlock, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is TextBlock tb)
        {
            tb.Classes.Add("hyperlink");
            tb.Cursor = new Cursor(StandardCursorType.Hand);
            tb.PointerPressed += (sender, args) =>
            {
                if (sender is TextBlock tb)
                {
                    if (tb.GetValue(UrlProperty) is string uri)
                    {
                        IOperatingSystem.Current.OpenUri(uri);
                    }
                }
            };
        }
    }

    private static void OnCommandSet(TextBlock textBlock, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Sender is TextBlock tb)
        {
            tb.Classes.Add("hyperlink");
            tb.Cursor = new Cursor(StandardCursorType.Hand);
            tb.PointerPressed += (sender, args) =>
            {
                if (sender is TextBlock tb)
                {
                    if (tb.GetValue(CommandProperty) is ICommand command)
                    {
                        object? parameter = tb.GetValue(CommandParameterProperty);
                        command.Execute(parameter);
                    }
                }
            };
        }
    }
}
