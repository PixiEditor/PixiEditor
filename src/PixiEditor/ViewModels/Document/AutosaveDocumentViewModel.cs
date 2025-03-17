using ColorPicker.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.Extensions.CommonApi.UserPreferences;
using PixiEditor.Helpers;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.DocumentModels.Autosave;
using PixiEditor.Models.IO;

namespace PixiEditor.ViewModels.Document;

internal class AutosaveDocumentViewModel : ObservableObject
{
    private AutosaveStateData? autosaveStateData;

    public AutosaveStateData? AutosaveStateData
    {
        get => autosaveStateData;
        set => SetProperty(ref autosaveStateData, value);
    }

    private bool currentDocumentAutosaveEnabled = true;

    public bool CurrentDocumentAutosaveEnabled
    {
        get => currentDocumentAutosaveEnabled;
        set
        {
            if (currentDocumentAutosaveEnabled == value)
                return;

            SetProperty(ref currentDocumentAutosaveEnabled, value);
            StopOrStartAutosaverIfNecessary();
        }
    }

    private DocumentAutosaver? autosaver;
    private DocumentViewModel Document { get; }
    private Guid autosaveFileGuid = Guid.NewGuid();
    public string AutosavePath => AutosaveHelper.GetAutosavePath(autosaveFileGuid);

    public string LastAutosavedPath { get; set; }

    private static bool SaveUserFileEnabled => IPreferences.Current!.GetPreference(
        PreferencesConstants.AutosaveToDocumentPath, PreferencesConstants.AutosaveToDocumentPathDefault);

    private static double AutosavePeriod =>
        IPreferences.Current!.GetPreference(PreferencesConstants.AutosavePeriodMinutes,
            PreferencesConstants.AutosavePeriodDefault);

    private static bool AutosaveEnabledGlobally =>
        IPreferences.Current!.GetPreference(PreferencesConstants.AutosaveEnabled,
            PreferencesConstants.AutosaveEnabledDefault);

    public AutosaveDocumentViewModel(DocumentViewModel document, DocumentInternalParts internals)
    {
        Document = document;
        internals.ChangeController.ToolSessionFinished += (() => autosaver?.OnUpdateableChangeEnded());
        IPreferences.Current!.AddCallback(PreferencesConstants.AutosaveEnabled, PreferenceUpdateCallback);
        IPreferences.Current!.AddCallback(PreferencesConstants.AutosavePeriodMinutes, PreferenceUpdateCallback);
        IPreferences.Current!.AddCallback(PreferencesConstants.AutosaveToDocumentPath, PreferenceUpdateCallback);
        StopOrStartAutosaverIfNecessary();
    }

    private void PreferenceUpdateCallback(string str, object obj)
    {
        StopOrStartAutosaverIfNecessary();
    }

    private void StopAutosaver()
    {
        autosaver?.Dispose();
        autosaver = null;
        AutosaveStateData = null;
    }

    private void StopOrStartAutosaverIfNecessary()
    {
        StopAutosaver();
        if (!AutosaveEnabledGlobally || !CurrentDocumentAutosaveEnabled)
            return;

        autosaver = new DocumentAutosaver(Document, TimeSpan.FromMinutes(AutosavePeriod), SaveUserFileEnabled);
        autosaver.JobChanged += (_, _) => AutosaveStateData = autosaver.State;
        AutosaveStateData = autosaver.State;
    }

    public bool Autosave(AutosaveHistoryType type)
    {
        if (Document.AllChangesSaved)
        {
            AddAutosaveHistoryEntry(
                type,
                AutosaveHistoryResult.NothingToSave);
            return true;
        }

        try
        {
            string filePath = AutosavePath;
            Directory.CreateDirectory(Directory.GetParent(filePath)!.FullName);
            ExportConfig config = new ExportConfig(Document.SizeBindable);
            bool success = Exporter.TrySave(Document, filePath, config, null) == SaveResult.Success;
            if (success)
            {
                AddAutosaveHistoryEntry(type, AutosaveHistoryResult.SavedBackup);
                LastAutosavedPath = filePath;
            }

            return success;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public bool AutosaveOnClose()
    {
        return Autosave(AutosaveHistoryType.OnClose);
    }

    public void AddAutosaveHistoryEntry(AutosaveHistoryType type, AutosaveHistoryResult result)
    {
        List<AutosaveHistorySession>? historySessions =
            IPreferences.Current!.GetLocalPreference<List<AutosaveHistorySession>>(PreferencesConstants
                .AutosaveHistory);
        if (historySessions is null)
            historySessions = new();

        AutosaveHistorySession currentSession;
        if (historySessions.Count == 0 || historySessions[^1].SessionGuid != ViewModelMain.Current.CurrentSessionId)
        {
            currentSession = new AutosaveHistorySession(ViewModelMain.Current.CurrentSessionId,
                ViewModelMain.Current.LaunchDateTime);
            historySessions.Add(currentSession);
        }
        else
        {
            currentSession = historySessions[^1];
        }

        AutosaveHistoryEntry entry = new(DateTime.Now, type, result, autosaveFileGuid, Document.FullFilePath);
        currentSession.AutosaveEntries.Add(entry);

        IPreferences.Current.UpdateLocalPreference(PreferencesConstants.AutosaveHistory, historySessions);
    }

    public void SetTempFileGuidAndLastSavedPath(Guid? guid, string lastSavedPath)
    {
        autosaveFileGuid = guid == null || guid.Value == Guid.Empty ? Guid.NewGuid() : guid.Value;
        LastAutosavedPath = lastSavedPath;
    }

    public void OnDocumentClosed()
    {
        CurrentDocumentAutosaveEnabled = false;
        IPreferences.Current!.RemoveCallback(PreferencesConstants.AutosaveEnabled, PreferenceUpdateCallback);
        IPreferences.Current!.RemoveCallback(PreferencesConstants.AutosavePeriodMinutes, PreferenceUpdateCallback);
        IPreferences.Current!.RemoveCallback(PreferencesConstants.AutosaveToDocumentPath, PreferenceUpdateCallback);
    }
}
