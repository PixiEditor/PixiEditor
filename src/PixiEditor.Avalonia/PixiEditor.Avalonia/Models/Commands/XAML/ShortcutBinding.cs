using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using PixiEditor.Helpers;
using PixiEditor.Models.DataHolders;
using ActualCommand = PixiEditor.Models.Commands.Commands.Command;

namespace PixiEditor.Models.Commands.XAML;

internal class ShortcutBinding : MarkupExtension
{
    private static CommandController commandController;

    public string Name { get; set; }

    public IValueConverter Converter { get; set; }
    
    public ShortcutBinding() { }

    public ShortcutBinding(string name) => Name = name;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (ViewModelMain.Current == null)
        {
            var attribute = DesignCommandHelpers.GetCommandAttribute(Name);
            return new KeyCombination(attribute.Key, attribute.Modifiers).ToString();
        }

        commandController ??= ViewModelMain.Current.CommandController;
        return GetBinding(commandController.Commands[Name], Converter).ProvideValue(serviceProvider);
    }

    public static Binding GetBinding(ActualCommand command, IValueConverter converter) => new Binding
    {
        Source = command,
        Path = new("Shortcut"),
        Mode = BindingMode.OneWay,
        StringFormat = "",
        Converter = converter
    };
}
