using Avalonia;
using Avalonia.Controls;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Models.Input;

namespace PixiEditor.AvaloniaUI.Models.Commands.XAML;

internal class Menu : global::Avalonia.Controls.Menu
{
    public static readonly AttachedProperty<string> CommandProperty =
        AvaloniaProperty.RegisterAttached<Menu, MenuItem, string>(nameof(Command));

    static Menu()
    {
        CommandProperty.Changed.Subscribe(CommandChanged);
    }

    public const double IconDimensions = 21;
    public static string GetCommand(MenuItem menu) => (string)menu.GetValue(CommandProperty);
    public static void SetCommand(MenuItem menu, string value) => menu.SetValue(CommandProperty, value);

    public static void CommandChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is not string value || e.Sender is not MenuItem item)
        {
            throw new InvalidOperationException($"{nameof(Menu)}.Command only works for MenuItem's");
        }

        if (Design.IsDesignMode)
        {
            HandleDesignMode(item, value);
            return;
        }

        var command = CommandController.Current.Commands[value];

        var icon = new Image
        {
            Source = command.GetIcon(), 
            Width = IconDimensions, Height = IconDimensions,
            Opacity = command.CanExecute() ? 1 : 0.75
        };

        icon.PropertyChanged += (sender, args) =>
        {
            if (args.Property.Name == nameof(icon.IsVisible))
            {
                icon.Opacity = command.CanExecute() ? 1 : 0.75;
            }
        };

        item.Command = Command.GetICommand(command, false);
        item.Icon = icon;
        item.Bind(MenuItem.InputGestureProperty, ShortcutBinding.GetBinding(command, null));
    }

    private static void HandleDesignMode(MenuItem item, string name)
    {
        var command = DesignCommandHelpers.GetCommandAttribute(name);
        item.InputGesture = new KeyCombination(command.Key, command.Modifiers).ToKeyGesture();
    }
}
