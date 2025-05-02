using PixiEditor.Extensions.CommonApi.UserPreferences;
using PixiEditor.Helpers;
using PixiEditor.Models;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Commands.Attributes.Evaluators;
using PixiEditor.Models.DocumentModels.Autosave;
using PixiEditor.Models.IO;
using PixiEditor.OperatingSystem;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.ViewModels.SubViewModels;

internal class AutosaveViewModel(ViewModelMain owner, DocumentManagerViewModel documentManager) : SubViewModel<ViewModelMain>(owner)
{
    public static bool SaveSessionStateEnabled => IPreferences.Current!.GetPreference(PreferencesConstants.SaveSessionStateEnabled, PreferencesConstants.SaveSessionStateDefault);

    [Command.Basic("PixiEditor.Autosave.OpenAutosaveFolder", "AUTOSAVE_OPEN_FOLDER", "AUTOSAVE_OPEN_FOLDER_DESCRIPTIVE",
        Icon = PixiPerfectIcons.Folder)]
    public void OpenAutosaveFolder()
    {
        if (Directory.Exists(Paths.PathToUnsavedFilesFolder))
        {
            IOperatingSystem.Current.OpenFolder(Paths.PathToUnsavedFilesFolder);
        }
    }

    [Command.Basic("PixiEditor.Autosave.ToggleAutosave", "AUTOSAVE_TOGGLE", "AUTOSAVE_TOGGLE_DESCRIPTIVE",
        CanExecute = "PixiEditor.Autosave.HasDocumentAndAutosaveEnabled")]
    public void ToggleAutosave()
    {
        var autosaveViewModel = documentManager.ActiveDocument!.AutosaveViewModel;

        autosaveViewModel.CurrentDocumentAutosaveEnabled = !autosaveViewModel.CurrentDocumentAutosaveEnabled;
    }

    [Evaluator.CanExecute("PixiEditor.Autosave.HasDocumentAndAutosaveEnabled")]
    public bool HasDocumentAndAutosaveEnabled() =>
        documentManager.DocumentNotNull() && IPreferences.Current!.GetPreference<bool>(PreferencesConstants.AutosaveEnabled);

    public void CleanupAutosavedFilesAndHistory()
    {
        if (!Directory.Exists(Paths.PathToUnsavedFilesFolder))
            return;

        List<AutosaveHistorySession>? autosaveHistory = IPreferences.Current!.GetLocalPreference<List<AutosaveHistorySession>>(PreferencesConstants.AutosaveHistory);
        if (autosaveHistory is null)
            autosaveHistory = new();

        if (autosaveHistory.Count > 3)
            autosaveHistory = autosaveHistory[^3..];

        foreach (var path in Directory.EnumerateFiles(Paths.PathToUnsavedFilesFolder))
        {
            try
            {
                Guid fileGuid = AutosaveHelper.GetAutosaveGuid(path)!.Value;
                bool lastWriteIsOld = (DateTime.Now - File.GetLastWriteTime(path)).TotalDays > Constants.MaxAutosaveFilesLifetimeDays;
                bool creationDateIsOld = (DateTime.Now - File.GetCreationTime(path)).TotalDays > Constants.MaxAutosaveFilesLifetimeDays;
                bool presentInHistory = autosaveHistory.Any(sess => sess.AutosaveEntries.Any(entry => entry.TempFileGuid == fileGuid));

                if (!presentInHistory && lastWriteIsOld && creationDateIsOld)
                    File.Delete(path);
            }
            catch (Exception e)
            {
                CrashHelper.SendExceptionInfo(e);
            }
        }
    }
}
