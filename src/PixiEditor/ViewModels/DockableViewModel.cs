using PixiDocks.Core.Docking;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.ViewModels;

internal abstract class DockableViewModel : ViewModelBase, IDockableContent
{
    public abstract string Id { get; }
    public abstract string Title { get; }
    public abstract bool CanFloat { get; }
    public abstract bool CanClose { get; }
    public TabCustomizationSettings TabCustomizationSettings { get; protected set; } = new();

    public DockableViewModel()
    {
        if (ILocalizationProvider.Current != null)
        {
            ILocalizationProvider.Current.OnLanguageChanged += OnLanguageChanged;
        }
    }

    private void OnLanguageChanged(Language language)
    {
        OnPropertyChanged(nameof(Title));
    }
}
