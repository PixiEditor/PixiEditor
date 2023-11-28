using System.IO;
using System.Windows.Threading;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.Common.UserPreferences;
using PixiEditor.Helpers;
using PixiEditor.Models.IO;
using PixiEditor.ViewModels.SubViewModels.Document;
using Timer = System.Timers.Timer;

namespace PixiEditor.Models.DocumentModels.Public;

internal class AutosaveViewModel : NotifyableObject
{
    private readonly Timer savingTimer;
    private readonly Timer updateTextTimer;
    private readonly Timer busyTimer;
    private bool saveAfterNextFinish;
    private bool reenableAfterNextSave;
    private string mainMenuText;
    private int savingFailed;
    private DateTime nextSave;
    private Guid tempGuid;
    
    private DocumentViewModel Document { get; }

    private double AutosavePeriodMinutes { get; set; } = -1;

    public LocalizedString MainMenuText
    {
        get => mainMenuText; 
        set => SetProperty(ref mainMenuText, value);
    }
    
    public AutosaveViewModel(DocumentViewModel document)
    {
        Document = document;
        tempGuid = Guid.NewGuid();
        savingTimer = new Timer();
        updateTextTimer = new Timer(TimeSpan.FromSeconds(10));

        savingTimer.Elapsed += (_, _) => TryAutosave();
        savingTimer.AutoReset = false;

        busyTimer = new Timer(TimeSpan.FromMilliseconds(80));
        busyTimer.AutoReset = false;
        busyTimer.Elapsed += (_, _) =>
        {
            Document.Busy = true;
        };

        updateTextTimer.Elapsed += (_, _) => SetAutosaveText();

        var preferences = IPreferences.Current;
        
        preferences.AddCallback<double>(PreferencesConstants.AutosavePeriodMinutes, AutosavePeriodChanged);
        AutosavePeriodChanged(preferences.GetPreference(PreferencesConstants.AutosavePeriodMinutes, PreferencesConstants.AutosavePeriodDefault));
    }

    public void HintFinishedAction()
    {
        if (!saveAfterNextFinish)
            return;

        saveAfterNextFinish = false;
        
        SafeAutosave();
    }

    public void HintSave()
    {
        if (!reenableAfterNextSave)
            return;

        reenableAfterNextSave = false;
        
        RestartTimers();
        SetAutosaveText();
    }

    private void SetAutosaveText()
    {
        var timeLeft = nextSave - DateTime.Now;

        if (timeLeft.Minutes == 0)
        {
            UpdateMainMenuTextSave("AUTOSAVE_SAVING_IN_MINUTE");
            return;
        }

        var adjusted = timeLeft.Add(TimeSpan.FromSeconds(30));
        
        var minute = adjusted.Minutes < 2
            ? new LocalizedString("MINUTE_SINGULAR")
            : new LocalizedString("MINUTE_PLURAL");

        UpdateMainMenuTextSave(new LocalizedString("AUTOSAVE_SAVING_IN", adjusted.Minutes.ToString(), minute));
    }

    private void TryAutosave()
    {
        if (Document.UpdateableChangeActive)
        {
            saveAfterNextFinish = true;
            UpdateMainMenuTextSave("AUTOSAVE_WAITING_FOR_SAVE");
            
            savingTimer.Stop();
            updateTextTimer.Stop();
            
            return;
        }

        if (Document.AllChangesSaved)
        {
            UpdateMainMenuTextSave("AUTOSAVE_SAVED");
            updateTextTimer.Stop();
            RestartTimers();
            return;
        }

        updateTextTimer.Stop();
        SafeAutosave();
    }

    private void SafeAutosave()
    {
        try
        {
            Autosave();
        }
        catch (Exception e)
        {
            savingFailed++;
            
            var minute = AutosavePeriodMinutes <= 1
                ? new LocalizedString("MINUTE_SINGULAR")
                : new LocalizedString("MINUTE_PLURAL");

            UpdateMainMenuTextSave(new LocalizedString("AUTOSAVE_FAILED_RETRYING", AutosavePeriodMinutes.ToString("0"), minute));
        
            busyTimer.Stop();
            Document.Busy = false;

            RestartTimers();

            if (savingFailed == 1)
            {
                CrashHelper.SendExceptionInfoToWebhook(e);
            }
        }
    }
    
    private void Autosave()
    {
        saveAfterNextFinish = false;
        
        UpdateMainMenuTextSave("AUTOSAVE_SAVING");

        string filePath;

        if (Document.FullFilePath == null || !Document.FullFilePath.EndsWith(".pixi"))
        {
            var root = Path.Combine(Path.GetTempPath(), "PixiEditor", "autosave");
            Directory.CreateDirectory(root);
            filePath = Path.Combine(root, $"autosave-{tempGuid}.pixi");
        }
        else
        {
            filePath = Document.FullFilePath;
        }
        
        busyTimer.Start();
        var result = Exporter.TrySave(Document, filePath);

        if (result == SaveResult.Success)
        {
            savingFailed = 0;
            UpdateMainMenuTextSave("AUTOSAVE_SAVED");
            Document.MarkAsSaved();
        }
        else if (result is SaveResult.InvalidPath or SaveResult.SecurityError)
        {
            UpdateMainMenuTextSave("AUTOSAVE_PLEASE_RESAVE");
            busyTimer.Stop();
            Document.Busy = false;
            reenableAfterNextSave = true;
            return;
        }
        else
        {
            busyTimer.Stop();
            Document.Busy = false;
            
            var minute = AutosavePeriodMinutes <= 1
                ? new LocalizedString("MINUTE_SINGULAR")
                : new LocalizedString("MINUTE_PLURAL");
            
            UpdateMainMenuTextSave(new LocalizedString("AUTOSAVE_FAILED_RETRYING", AutosavePeriodMinutes.ToString("0"), minute));
            savingFailed++;

            if (savingFailed == 3)
            {
                CrashHelper.SendExceptionInfoToWebhook(new Exception($"Failed to autosave 3 times in a row due to {result}"));
            }
        }
        
        busyTimer.Stop();
        Document.Busy = false;

        RestartTimers();
        
    }

    private void RestartTimers()
    {
        savingTimer.Start();
        nextSave = DateTime.Now + TimeSpan.FromMilliseconds(savingTimer.Interval);
        updateTextTimer.Start();
    }

    private void AutosavePeriodChanged(double minutes)
    {
        if (minutes == -1)
        {
            savingTimer.Enabled = false;
            updateTextTimer.Enabled = false;
            saveAfterNextFinish = false;
            
            UpdateMainMenuTextSave(string.Empty);

            AutosavePeriodMinutes = minutes;
            return;
        }
        
        var timerEnabled = savingTimer.Enabled;

        if (AutosavePeriodMinutes == -1)
        {
            timerEnabled = true;
            updateTextTimer.Start();
        }
        
        savingTimer.Enabled = false;

        var timeSpan = TimeSpan.FromMinutes(minutes);
        savingTimer.Interval = timeSpan.TotalMilliseconds;
        AutosavePeriodMinutes = minutes;
        
        savingTimer.Enabled = timerEnabled;
        
        nextSave = DateTime.Now + timeSpan;
        if (updateTextTimer.Enabled)
        {
            SetAutosaveText();
        }
    }

    private void UpdateMainMenuTextSave(LocalizedString text)
    {
        Dispatcher.CurrentDispatcher.Invoke(() =>
        {
            MainMenuText = text;
        });
    }
}
