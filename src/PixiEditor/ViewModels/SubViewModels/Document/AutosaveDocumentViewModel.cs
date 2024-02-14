using System.IO;
using System.Windows;
using System.Windows.Threading;
using PixiEditor.Extensions.Common.UserPreferences;
using PixiEditor.Helpers;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.IO;
using PixiEditor.Views.UserControls;

namespace PixiEditor.ViewModels.SubViewModels.Document;


internal class AutosaveDocumentViewModel : NotifyableObject
{
    private readonly DispatcherTimer savingTimer;
    private readonly DispatcherTimer busyTimer;
    private int savingFailed;
    private Guid tempGuid;
    private bool autosaveEnabled = true;
    private bool waitingForUpdateableChangeEnd = false;
    private LastAutosaveData? lastAutosaveData = null;
    private DateTime? autosaveLaunchDateTime = null;
    
    private DocumentViewModel Document { get; }
    
    private double AutosavePeriodMinutes { get; set; } = -1;
    
    private AutosaveStateData autosaveStateData;
    public AutosaveStateData AutosaveStateData
    {
        get => autosaveStateData;
        set => SetProperty(ref autosaveStateData, value);
    }

    public bool Enabled
    {
        get => autosaveEnabled;
        set
        {
            if (autosaveEnabled == value)
                return;
            
            AutosavePeriodChanged(
                IPreferences.Current!.GetPreference(
                    PreferencesConstants.AutosavePeriodMinutes, 
                    PreferencesConstants.AutosavePeriodDefault),
                value);
            SetProperty(ref autosaveEnabled, value);
        }
    }

    public string LastSavedPath { get; private set; }
    
    public static bool SaveStateEnabled => IPreferences.Current!.GetPreference(PreferencesConstants.SaveSessionStateEnabled, PreferencesConstants.SaveSessionStateDefault);
    
    private bool SaveToDocumentPath => IPreferences.Current!.GetPreference(PreferencesConstants.AutosaveToDocumentPath, PreferencesConstants.AutosaveToDocumentPathDefault);
    
    public AutosaveDocumentViewModel(DocumentViewModel document, DocumentInternalParts internals)
    {
        Document = document;
        tempGuid = Guid.NewGuid();

        var dispatcher = Application.Current.Dispatcher;
        
        savingTimer = new DispatcherTimer(DispatcherPriority.Normal);
        savingTimer.Tick += (_, _) =>
        {
            savingTimer.Stop(); 
            TryAutosave();
        };

        busyTimer = new DispatcherTimer(DispatcherPriority.Normal, dispatcher) { Interval = TimeSpan.FromMilliseconds(80) };
        busyTimer.Tick += (_, _) =>
        {
            busyTimer!.Stop();
            Document.Busy = true;
        };

        internals.ChangeController.UpdateableChangeEnded += OnUpdateableChangeEnded;

        var preferences = IPreferences.Current;
        
        preferences!.AddCallback<double>(PreferencesConstants.AutosavePeriodMinutes, (v) => AutosavePeriodChanged(v, autosaveEnabled));
        AutosavePeriodChanged(preferences.GetPreference(PreferencesConstants.AutosavePeriodMinutes, PreferencesConstants.AutosavePeriodDefault), autosaveEnabled);
    }

    private AutosaveStateData CreateAutosaveStateData()
    {
        return new AutosaveStateData
        {
            LastAutosaveData = lastAutosaveData,
            AutosaveLaunchDateTime = autosaveLaunchDateTime ?? DateTime.Now,
            AutosaveInterval = TimeSpan.FromMinutes(AutosavePeriodMinutes),
            AutosaveState = AutosaveState.Paused
        };
    }

    public static void AutosaveOnClose()
    {
        
    }

    public void TryAutosave(bool saveUserFileIfEnabled = true)
    {
        if (Document.AllChangesSaved)
        {
            RestartTimers();
            
            lastAutosaveData = new LastAutosaveData()
            {
                Time = DateTime.Now,
                BackupSaveResult = BackupAutosaveResult.NothingToSave,
                UserFileSaveResult = UserFileAutosaveResult.NothingToSave
            };
            AutosaveStateData = CreateAutosaveStateData() with { AutosaveState = AutosaveState.Idle };
            return;
        }

        if (Document.UpdateableChangeActive)
        {
            waitingForUpdateableChangeEnd = true;
            return;
        }

        SafeAutosave(saveUserFileIfEnabled);
    }

    public void PanicAutosaveFromDeadlockDetector()
    {
        string filePath = Path.Join(Paths.PathToUnsavedFilesFolder, $"autosave-{tempGuid}.pixi");
        Directory.CreateDirectory(Directory.GetParent(filePath)!.FullName);

        var result = Exporter.TrySave(Document, filePath);

        if (result == SaveResult.Success)
        {
            LastSavedPath = filePath;
        }
    }

    private async void SafeAutosave(bool saveUserFile)
    {
        try
        {
            await Autosave(saveUserFile);
        }
        catch (Exception e)
        {
            savingFailed++;
            
            lastAutosaveData = new LastAutosaveData()
            {
                Time = DateTime.Now,
                BackupSaveResult = BackupAutosaveResult.Error,
                UserFileSaveResult = UserFileAutosaveResult.ExceptionWhileSaving
            };
            AutosaveStateData = CreateAutosaveStateData() with { AutosaveState = AutosaveState.Idle };
            
            busyTimer.Stop();
            Document.Busy = false;

            RestartTimers();

            if (savingFailed == 1)
            {
                CrashHelper.SendExceptionInfoToWebhook(e);
            }
        }
    }
    
    private async Task Autosave(bool saveUserFile)
    {
        AutosaveStateData = CreateAutosaveStateData() with { AutosaveState = AutosaveState.InProgress };

        string filePath = Path.Join(Paths.PathToUnsavedFilesFolder, $"autosave-{tempGuid}.pixi");
        Directory.CreateDirectory(Directory.GetParent(filePath)!.FullName);
        
        busyTimer.Start();
        var result = Exporter.TrySave(Document, filePath);

        UserFileAutosaveResult userFileSaveResult = UserFileAutosaveResult.Disabled;
        
        if (result == SaveResult.Success)
        {
            if (saveUserFile && SaveToDocumentPath && Document.FullFilePath != null)
            {
                userFileSaveResult = await CopyTempToUserFile(filePath);
            }
            
            lastAutosaveData = new LastAutosaveData
            {
                Time = DateTime.Now,
                BackupSaveResult = BackupAutosaveResult.Success,
                UserFileSaveResult = userFileSaveResult
            };
            AutosaveStateData = CreateAutosaveStateData() with { AutosaveState = AutosaveState.Idle, LastAutosaveData = lastAutosaveData };
            
            Document.MarkAsAutosaved();
            LastSavedPath = filePath;
        }
        else
        {
            busyTimer.Stop();
            Document.Busy = false;
            
            lastAutosaveData = new LastAutosaveData()
            {
                Time = DateTime.Now,
                BackupSaveResult = BackupAutosaveResult.Error,
                UserFileSaveResult = userFileSaveResult
            };    
            AutosaveStateData = CreateAutosaveStateData() with { AutosaveState = AutosaveState.Idle, LastAutosaveData = lastAutosaveData };
            
            savingFailed++;

            if (savingFailed < 3)
            {
                int savingFailedCopy = savingFailed;
                Task.Run(() => CrashHelper.SendExceptionInfoToWebhook(new Exception($"Failed to autosave for the {savingFailedCopy}. time due to {result}")));
            }
        }
        
        busyTimer.Stop();
        Document.Busy = false;

        RestartTimers();
    }

    private async Task<UserFileAutosaveResult> CopyTempToUserFile(string tempPath)
    {
        if (!File.Exists(Document.FullFilePath))
            return UserFileAutosaveResult.NoUserFile;
        
        var result = await Task.Run(Copy);
        Document.MarkAsSaved();
        return result;
        
        UserFileAutosaveResult Copy()
        {
            try
            {
                File.Copy(tempPath, Document.FullFilePath!, true);
                return UserFileAutosaveResult.Success;
            }
            catch (Exception e) when (e is UnauthorizedAccessException or DirectoryNotFoundException)
            {
                return UserFileAutosaveResult.NoUserFile;
            }
            catch
            {
                return UserFileAutosaveResult.ExceptionWhileSaving;
            }
        }
    }

    private void RestartTimers()
    {
        savingTimer.Start();
    }

    private void AutosavePeriodChanged(double minutes, bool documentEnabled)
    {
        if ((int)minutes == -1 || !documentEnabled)
        {
            AutosavePeriodMinutes = minutes;
            AutosaveStateData = CreateAutosaveStateData();
            return;
        }
        
        var timerEnabled = savingTimer.IsEnabled || (int)AutosavePeriodMinutes == -1 || !Enabled;

        savingTimer.IsEnabled = false;

        var timeSpan = TimeSpan.FromMinutes(minutes);
        savingTimer.Interval = timeSpan;
        AutosavePeriodMinutes = minutes;
        
        savingTimer.IsEnabled = timerEnabled;
    }

    private void OnUpdateableChangeEnded(object? sender, EventArgs args)
    {
        
    }

    public void SetTempFileGuidAndLastSavedPath(Guid guid, string lastSavedPath)
    {
        tempGuid = guid;
        LastSavedPath = lastSavedPath;
    }
}
