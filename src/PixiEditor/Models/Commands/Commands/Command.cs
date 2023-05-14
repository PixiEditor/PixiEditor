using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using PixiEditor.Models.Commands.Evaluators;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Localization;

namespace PixiEditor.Models.Commands.Commands;

[DebuggerDisplay("{InternalName,nq} ('{DisplayName,nq}')")]
internal abstract partial class Command : NotifyableObject
{
    private KeyCombination _shortcut;

    public bool IsDebug { get; init; }

    public string InternalName { get; init; }

    public string IconPath { get; init; }

    public IconEvaluator IconEvaluator { get; init; }

    public LocalizedString DisplayName { get; set; }

    public LocalizedString Description { get; set; }

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

    public abstract object GetParameter();

    protected Command(Action<object> onExecute, CanExecuteEvaluator canExecute)
    {
        Methods = new(this, onExecute, canExecute);
        ILocalizationProvider.Current.OnLanguageChanged += OnLanguageChanged;
        InputLanguageManager.Current.InputLanguageChanged += (_, _) => RaisePropertyChanged(nameof(Shortcut));
    }

    private void OnLanguageChanged(Language obj)
    {
        DisplayName = new LocalizedString(DisplayName.Key, DisplayName.Parameters);
        Description = new LocalizedString(Description.Key, Description.Parameters);

        RaisePropertyChanged(nameof(DisplayName));
        RaisePropertyChanged(nameof(Description));
    }

    public void Execute() => Methods.Execute(GetParameter());

    public bool CanExecute() => Methods.CanExecute(GetParameter());

    public ImageSource GetIcon() => IconEvaluator.CallEvaluate(this, GetParameter());

    public delegate void ShortcutChangedEventHandler(Command command, ShortcutChangedEventArgs args);
}
