using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.AvaloniaUI.Models.Input;
using ActualCommand = PixiEditor.AvaloniaUI.Models.Commands.Commands.Command;

namespace PixiEditor.AvaloniaUI.Models.Commands.XAML;

internal class ShortcutBinding : MarkupExtension
{
    private static CommandController commandController;

    public string Name { get; set; }

    public IValueConverter Converter { get; set; }
    
    public ShortcutBinding() { }

    public ShortcutBinding(string name) => Name = name;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (Design.IsDesignMode)
        {
            var attribute = DesignCommandHelpers.GetCommandAttribute(Name);
            return new KeyCombination(attribute.Key, attribute.Modifiers).ToString();
        }

        ICommandsHandler? handler = serviceProvider.GetService<ICommandsHandler>();
        commandController ??= handler.CommandController;
        var binding = GetBinding(commandController.Commands[Name], Converter);

        var targetValue = serviceProvider.GetService<IProvideValueTarget>();
        var targetObject = targetValue.TargetObject as AvaloniaObject;
        var targetProperty = targetValue.TargetProperty as AvaloniaProperty;

        var instancedBinding = binding.Initiate(targetObject, targetProperty);

        return instancedBinding; //TODO: This won't work, leaving it for now
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
