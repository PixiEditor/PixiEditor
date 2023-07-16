using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Helpers;
using ReactiveUI;

namespace PixiEditor.Models.Commands.XAML;

internal class Command : MarkupExtension
{
    private static CommandController commandController;

    public string Name { get; set; }

    public bool UseProvided { get; set; }

    public bool GetPixiCommand { get; set; }

    public Command() { }

    public Command(string name) => Name = name;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (Design.IsDesignMode)
        {
            var attribute = DesignCommandHelpers.GetCommandAttribute(Name);
            return GetICommand(
                new Commands.Command.BasicCommand(null, null)
                {
                    InternalName = Name,
                    DisplayName = attribute.DisplayName,
                    Description = attribute.Description,
                    DefaultShortcut = attribute.GetShortcut(),
                    Shortcut = attribute.GetShortcut()
                }, false);
        }

        if (commandController is null)
        {
            commandController = serviceProvider.GetRequiredService<CommandController>();
        }

        var command = commandController.Commands[Name];
        return GetPixiCommand ? command : GetICommand(command, UseProvided);
    }

    public static IReactiveCommand GetICommand(Commands.Command command, bool useProvidedParameter) => new ProvidedICommand()
    {
        Command = command,
        UseProvidedParameter = useProvidedParameter,
    };

    class ProvidedICommand : IReactiveCommand
    {
        // TODO: Implement with ReactiveUI
        /*public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public Commands.Command Command { get; init; }

        public bool UseProvidedParameter { get; init; }

        public bool CanExecute(object parameter) => UseProvidedParameter ? Command.Methods.CanExecute(parameter) : Command.CanExecute();

        public void Execute(object parameter)
        {
            if (UseProvidedParameter)
            {
                Command.Methods.Execute(parameter);
            }
            else
            {
                Command.Execute();
            }
        }*/
    }
}
