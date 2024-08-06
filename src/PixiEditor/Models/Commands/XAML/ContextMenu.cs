using Avalonia;
using Avalonia.Controls;
using PixiEditor.Helpers;
using PixiEditor.Models.Commands.CommandContext;
using PixiEditor.Models.Input;

namespace PixiEditor.Models.Commands.XAML;

internal class ContextMenu : global::Avalonia.Controls.ContextMenu
{
    public static readonly StyledProperty<string> CommandNameProperty =
        AvaloniaProperty.Register<Menu, string>(nameof(Command));

    static ContextMenu()
    {
        CommandNameProperty.Changed.Subscribe(CommandChanged);
    }

    public static string GetCommand(ContextMenu target) => (string)target.GetValue(CommandNameProperty);

    public static void SetCommand(ContextMenu target, string value) => target.SetValue(CommandNameProperty, value);

    public static void CommandChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is not string value || e.Sender is not MenuItem item)
        {
            throw new InvalidOperationException($"{nameof(ContextMenu)}.Command only works for MenuItem's");
        }

        if (Design.IsDesignMode)
        {
            HandleDesignMode(item, value);
            return;
        }

        var command = CommandController.Current.Commands[value];

        item.Command = Command.GetICommand(command, new MenuSourceInfo(MenuType.ContextMenu), false);
        item.Bind(MenuItem.InputGestureProperty, ShortcutBinding.GetBinding(command, null, true));
    }

    private static void HandleDesignMode(MenuItem item, string name)
    {
        var command = DesignCommandHelpers.GetCommandAttribute(name);
        item.InputGesture = new KeyCombination(command.Key, command.Modifiers).ToKeyGesture();
    }
}
