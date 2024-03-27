using PixiEditor.Extensions.Common.UserPreferences;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.ViewModels.SubViewModels.Main;

internal class AutosaveViewModel : SubViewModel<ViewModelMain>
{
    private DocumentManagerViewModel _documentManager;
    
    public AutosaveViewModel(ViewModelMain owner, DocumentManagerViewModel documentManager) : base(owner)
    {
        _documentManager = documentManager;
    }
    
    [Command.Basic("PixiEditor.Autosave.ToggleAutosave", "AUTOSAVE_TOGGLE", "AUTOSAVE_TOGGLE_DESCRIPTION", CanExecute = "PixiEditor.Autosave.HasDocumentAndAutosaveEnabled")]
    public void ToggleAutosave()
    {
        var autosaveViewModel = _documentManager.ActiveDocument!.AutosaveViewModel;

        autosaveViewModel.Enabled = !autosaveViewModel.Enabled;
    }

    [Evaluator.CanExecute("PixiEditor.Autosave.HasDocumentAndAutosaveEnabled")]
    public bool HasDocumentAndAutosaveEnabled() => 
        _documentManager.DocumentNotNull() &&
        (int)IPreferences.Current.GetPreference<double>(PreferencesConstants.AutosavePeriodMinutes) != -1;
}
