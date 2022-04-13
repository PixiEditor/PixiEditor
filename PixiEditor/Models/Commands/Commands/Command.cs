using PixiEditor.Helpers;
using PixiEditor.Models.Commands.Evaluators;
using PixiEditor.Models.DataHolders;
using System.Diagnostics;
using System.Windows.Media;

namespace PixiEditor.Models.Commands
{
    [DebuggerDisplay("{Name,nq} ('{Display,nq}')")]
    public abstract partial class Command : NotifyableObject
    {
        private KeyCombination _shortcut;

        public bool IsDebug { get; init; }

        public string Name { get; init; }

        public string IconPath { get; init; }

        public IconEvaluator IconEvaluator { get; init; }

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

        protected Command(Action<object> onExecute, CanExecuteEvaluator canExecute) =>
            Methods = new(this, onExecute, canExecute);

        public void Execute() => Methods.Execute(GetParameter());

        public bool CanExecute() => Methods.CanExecute(GetParameter());

        public ImageSource GetIcon() => IconEvaluator.EvaluateEvaluator(this, GetParameter());
    }
}
