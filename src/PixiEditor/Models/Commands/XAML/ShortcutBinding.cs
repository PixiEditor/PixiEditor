using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using PixiEditor.Helpers;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Input;
using PixiEditor.ViewModels;
using ActualCommand = PixiEditor.Models.Commands.Commands.Command;

namespace PixiEditor.Models.Commands.XAML;

internal class ShortcutBinding : MarkupExtension
{
    private static CommandController commandController;

    public string Name { get; set; }
    
    public bool UseAvaloniaGesture { get; set; } = true;

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

        ICommandsHandler? handler = ViewModelMain.Current;
        commandController ??= handler.CommandController;
        return GetBinding(commandController.Commands[Name], Converter, UseAvaloniaGesture);

        /*var targetValue = serviceProvider.GetService<IProvideValueTarget>();
        var targetObject = targetValue.TargetObject as AvaloniaObject;
        var targetProperty = targetValue.TargetProperty as AvaloniaProperty;

        var instancedBinding = binding.Initiate(targetObject, targetProperty);

        return instancedBinding; //TODO: This won't work, leaving it for now*/
    }

    public static Binding GetBinding(ActualCommand command, IValueConverter converter, bool useAvaloniaGesture) => new Binding
    {
        Source = command,
        Path = useAvaloniaGesture ? "Shortcut.Gesture" : "Shortcut",
        Mode = BindingMode.OneWay,
        StringFormat = "",
        Converter = converter
    };
}
