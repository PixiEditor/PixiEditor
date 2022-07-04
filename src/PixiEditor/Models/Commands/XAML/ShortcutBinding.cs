using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Helpers;
using PixiEditor.Models.DataHolders;
using PixiEditor.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using ActualCommand = PixiEditor.Models.Commands.Command;

namespace PixiEditor.Models.Commands.XAML;

public class ShortcutBinding : MarkupExtension
{
    private static CommandController commandController;

    public string Name { get; set; }

    public ShortcutBinding() { }

    public ShortcutBinding(string name) => Name = name;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (commandController == null)
        {
            commandController = ViewModelMain.Current.CommandController;
        }

        if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
        {
            var attribute = DesignCommandHelpers.GetCommandAttribute(Name);
            return new KeyCombination(attribute.Key, attribute.Modifiers).ToString();
        }

        return GetBinding(commandController.Commands[Name]).ProvideValue(serviceProvider);
    }

    public static Binding GetBinding(ActualCommand command) => new Binding
    {
        Source = command,
        Path = new("Shortcut"),
        Mode = BindingMode.OneWay,
        StringFormat = ""
    };
}