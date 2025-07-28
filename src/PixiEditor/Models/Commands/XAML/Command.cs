using System.Collections.Generic;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PixiEditor.Helpers;
using PixiEditor.Models.Commands.CommandContext;

namespace PixiEditor.Models.Commands.XAML;

internal class Command : MarkupExtension
{
    private static CommandController commandController;

    public string Name { get; set; }

    public bool UseProvided { get; set; }

    public bool GetPixiCommand { get; set; }
    
    public string SourceInfoTag { get; set; }

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
                }, null, false);
        }

        if (commandController is null)
        {
            commandController = CommandController.Current;
        }

        bool contains = commandController.Commands.ContainsKey(Name);

        if (!contains)
        {
            return null;
        }

        Commands.Command command = commandController.Commands[Name];
        return GetPixiCommand ? command : GetICommand(command, new CommandBindingSourceInfo(SourceInfoTag), UseProvided);
    }

    public static ICommand GetICommand(Commands.Command command, ICommandExecutionSourceInfo? source, bool useProvidedParameter) => new ProvidedICommand()
    {
        Command = command,
        UseProvidedParameter = useProvidedParameter,
        Source = source
    };

    class ProvidedICommand : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add
            {
                if (CanExecuteChangedHandlers.Count == 0)
                {
                    CommandController.ListenForCanExecuteChanged(Command);
                }

                CanExecuteChangedHandlers.Add(value);
            }
            remove
            {
                CanExecuteChangedHandlers.Remove(value);
                if (CanExecuteChangedHandlers.Count == 0)
                {
                    CommandController.StopListeningForCanExecuteChanged(Command);
                }
            }
        }

        private List<EventHandler> CanExecuteChangedHandlers { get; } = new();

        private Commands.Command command;

        public Commands.Command Command
        {
            get => command;
            init
            {
                command = value;
                Command.CanExecuteChanged += () => CanExecuteChangedHandlers.ForEach(x => x.Invoke(this, EventArgs.Empty));
            }
        }

        public bool UseProvidedParameter { get; init; }

        public ICommandExecutionSourceInfo Source { get; init; }
        
        public bool CanExecute(object parameter) => UseProvidedParameter ? Command.Methods.CanExecute(parameter) : Command.CanExecute();

        public void Execute(object parameter)
        {
            var context = new CommandExecutionContext(parameter, Source);
            Command.Execute(context, UseProvidedParameter);
        }
    }
}
