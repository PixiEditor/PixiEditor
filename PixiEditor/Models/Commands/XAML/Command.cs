using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Helpers;
using PixiEditor.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace PixiEditor.Models.Commands.XAML
{
    public class Command : MarkupExtension
    {
        private static CommandController commandController;

        public string Name { get; set; }

        public bool UseProvided { get; set; }

        public bool GetPixiCommand { get; set; }

        public Command() { }

        public Command(string name) => Name = name;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (commandController == null)
            {
                commandController = ViewModelMain.Current.CommandController;
            }

            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                var attribute = DesignCommandHelpers.GetCommandAttribute(Name);
                return GetICommand(
                    new Commands.Command.BasicCommand(null, null)
                    {
                        Name = Name,
                        Display = attribute.DisplayName,
                        Description = attribute.Description,
                        DefaultShortcut = attribute.GetShortcut(),
                        Shortcut = attribute.GetShortcut()
                    }, false);
            }

            var command = commandController.Commands[Name];
            return GetPixiCommand ? command : GetICommand(command, UseProvided);
        }

        public static ICommand GetICommand(Commands.Command command, bool useProvidedParameter) => new ProvidedICommand()
        {
            Command = command,
            UseProvidedParameter = useProvidedParameter,
        };

        class ProvidedICommand : ICommand
        {
            public event EventHandler CanExecuteChanged
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
            }
        }
    }
}
