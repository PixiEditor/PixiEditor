using PixiEditor.Helpers;
using PixiEditor.Models.DataHolders;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace PixiEditor.Models.Commands.XAML;

public class Menu : System.Windows.Controls.Menu
{
    public static readonly DependencyProperty CommandNameProperty =
        DependencyProperty.RegisterAttached(
            "Command",
            typeof(string),
            typeof(Menu),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, CommandChanged)
        );

    public static string GetCommand(UIElement target) => (string)target.GetValue(CommandNameProperty);

    public static void SetCommand(UIElement target, string value) => target.SetValue(CommandNameProperty, value);

    public static void CommandChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not string value || sender is not MenuItem item)
        {
            throw new InvalidOperationException($"{nameof(Menu)}.Command only works for MenuItem's");
        }

        if (DesignerProperties.GetIsInDesignMode(sender as DependencyObject))
        {
            HandleDesignMode(item, value);
            return;
        }

        var command = CommandController.Current.Commands[value];

        item.Command = Command.GetICommand(command, false);
        item.SetBinding(MenuItem.InputGestureTextProperty, ShortcutBinding.GetBinding(command));
    }

    private static void HandleDesignMode(MenuItem item, string name)
    {
        var command = DesignCommandHelpers.GetCommandAttribute(name);
        item.InputGestureText = new KeyCombination(command.Key, command.Modifiers).ToString();
    }
}