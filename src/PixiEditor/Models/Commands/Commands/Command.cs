using System.Diagnostics;
using System.Windows.Media;
using PixiEditor.Models.Commands.Evaluators;
using PixiEditor.Models.DataHolders;

namespace PixiEditor.Models.Commands.Commands;

[DebuggerDisplay("{InternalName,nq} ('{DisplayName,nq}')")]
internal abstract partial class Command : NotifyableObject
{
    private KeyCombination _shortcut;

    public bool IsDebug { get; init; }

    public string InternalName { get; init; }

    public string IconPath { get; init; }

    public IconEvaluator IconEvaluator { get; init; }

    public string DisplayName { get; init; }

    public string Description { get; init; }

    public CommandMethods Methods { get; init; }

    public KeyCombination DefaultShortcut { get; init; }

    public KeyCombination Shortcut
    {
        get => _shortcut;
        set
        {
            if (SetProperty(ref _shortcut, value, out var oldValue))
            {
                ShortcutChanged?.Invoke(this, new(oldValue, value));
            }
        }
    }

    public event ShortcutChangedEventHandler ShortcutChanged;

    protected abstract object GetParameter();

    protected Command(Action<object> onExecute, CanExecuteEvaluator canExecute) =>
        Methods = new(this, onExecute, canExecute);

    public void Execute() => Methods.Execute(GetParameter());

    public bool CanExecute() => Methods.CanExecute(GetParameter());

    public ImageSource GetIcon() => IconEvaluator.CallEvaluate(this, GetParameter());

    public delegate void ShortcutChangedEventHandler(Command command, ShortcutChangedEventArgs args);
}
