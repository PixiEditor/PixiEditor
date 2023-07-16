using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using Avalonia.Media;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Commands.Evaluators;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Localization;
using ReactiveUI;

namespace PixiEditor.Models.Commands.Commands;

[DebuggerDisplay("{InternalName,nq} ('{DisplayName,nq}')")]
internal abstract partial class Command : ReactiveObject
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
            var oldValue = _shortcut;
            var combination = this.RaiseAndSetIfChanged(ref _shortcut, value);
            if (combination != oldValue)
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
        /*InputLanguageManager.Current.InputLanguageChanged += (_, _) => this.RaisePropertyChanged(nameof(Shortcut)); TODO: Didn't find implementation of this in Avalonia*/
    }

    private void OnLanguageChanged(Language obj)
    {
        DisplayName = new LocalizedString(DisplayName.Key, DisplayName.Parameters);
        Description = new LocalizedString(Description.Key, Description.Parameters);

        this.RaisePropertyChanged(nameof(DisplayName));
        this.RaisePropertyChanged(nameof(Description));
    }

    public void Execute() => Methods.Execute(GetParameter());

    public bool CanExecute() => Methods.CanExecute(GetParameter());

    public IImage GetIcon() => IconEvaluator.CallEvaluate(this, GetParameter());

    public delegate void ShortcutChangedEventHandler(Command command, ShortcutChangedEventArgs args);
}
