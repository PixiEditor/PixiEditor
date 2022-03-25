using PixiEditor.Helpers;
using PixiEditor.Models.DataHolders;
using System.Diagnostics;
using System.Windows.Input;

namespace PixiEditor.Models.Commands
{
    [DebuggerDisplay("{Name,nq} ('{Display,nq}')")]
    public abstract partial class Command : NotifyableObject
    {
        private KeyCombination _shortcut;

        public string Name { get; init; }

        public string Display { get; init; }

        public string Description { get; init; }

        public CommandMethods Methods { get; init; }

        public KeyCombination DefaultShortcut { get; init; }

        public KeyCombination Shortcut
        {
            get => _shortcut;
            set => SetProperty(ref _shortcut, value);
        }

        protected abstract object GetParameter();

        public void Execute() => Methods.Execute(GetParameter());

        public bool CanExecute() => Methods.CanExecute(GetParameter());

        public ICommand GetICommand(bool useProvidedParameter) => new ProvidedICommand()
        {
            Command = this,
            UseProvidedParameter = useProvidedParameter,
        };

        class ProvidedICommand : ICommand
        {
            public event EventHandler CanExecuteChanged
            {
                add => CommandManager.RequerySuggested += value;
                remove => CommandManager.RequerySuggested -= value;
            }

            public Command Command { get; init; }

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
