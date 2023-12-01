using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.Common.UserPreferences;
using PixiEditor.Helpers;
using PixiEditor.Models.IO;
using PixiEditor.ViewModels.SubViewModels.Document;
using Timer = System.Timers.Timer;

namespace PixiEditor.Models.DocumentModels.Public;

internal class AutosaveDocumentViewModel : NotifyableObject
{
    private readonly Timer savingTimer;
    private readonly Timer updateTextTimer;
    private readonly Timer busyTimer;
    private bool saveAfterNextFinish;
    private bool reenableAfterNextSave;
    private int savingFailed;
    private DateTime nextSave;
    private Guid tempGuid;
    private bool documentEnabled = true;

    private const string ClockIcon = "\ue84d";
    private const string WarnIcon = "\ue81e";
    private const string SaveIcon = "\ue8bc";
    private const string PauseIcon = "\ue8a2";
    private const string SavingIcon = "\ue864";

    private readonly Brush ErrorBrush = new SolidColorBrush(Color.FromArgb(255, 214, 66, 56));
    private readonly Brush WarnBrush = new SolidColorBrush(Color.FromArgb(255, 219, 189, 53));
    private readonly Brush SuccessBrush = new SolidColorBrush(Color.FromArgb(255, 83, 207, 72));
    private readonly Brush ActiveBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
    private readonly Brush InactiveBrush = new SolidColorBrush(Color.FromArgb(255, 120, 120, 120));
    
    private DocumentViewModel Document { get; }

    private double AutosavePeriodMinutes { get; set; } = -1;

    private LocalizedString mainMenuText;
    
    public LocalizedString MainMenuText
    {
        get => mainMenuText; 
        set => SetProperty(ref mainMenuText, value);
    }

    private string mainMenuIconText;

    public string MainMenuIconText
    {
        get => mainMenuIconText;
        set => SetProperty(ref mainMenuIconText, value);
    }

    private Brush mainMenuBrush;

    public Brush MainMenuBrush
    {
        get => mainMenuBrush;
        set => SetProperty(ref mainMenuBrush, value);
    }

    private bool mainMenuPulse;

    public bool MainMenuPulse
    {
        get => mainMenuPulse;
        set => SetProperty(ref mainMenuPulse, value);
    }

    public bool Enabled
    {
        get => documentEnabled;
        set
        {
            if (documentEnabled == value)
                return;
            
            AutosavePeriodChanged(
                IPreferences.Current.GetPreference(
                    PreferencesConstants.AutosavePeriodMinutes, 
                    PreferencesConstants.AutosavePeriodDefault),
                value);
            SetProperty(ref documentEnabled, value);
        }
    }

    public string LastSavedPath { get; private set; }
    
    public AutosaveDocumentViewModel(DocumentViewModel document)
    {
        Document = document;
        tempGuid = Guid.NewGuid();
        savingTimer = new Timer();
        updateTextTimer = new Timer(TimeSpan.FromSeconds(3));

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
        
        preferences.AddCallback<double>(PreferencesConstants.AutosavePeriodMinutes, (v) => AutosavePeriodChanged(v, documentEnabled));
        AutosavePeriodChanged(preferences.GetPreference(PreferencesConstants.AutosavePeriodMinutes, PreferencesConstants.AutosavePeriodDefault), documentEnabled);
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
            UpdateMainMenuTextSave("AUTOSAVE_SAVING_IN_MINUTE", ClockIcon, InactiveBrush, false);
            return;
        }

        var adjusted = timeLeft.Add(TimeSpan.FromSeconds(30));
        
        var minute = adjusted.Minutes < 2
            ? new LocalizedString("MINUTE_SINGULAR")
            : new LocalizedString("MINUTE_PLURAL");

        UpdateMainMenuTextSave(new LocalizedString("AUTOSAVE_SAVING_IN", adjusted.Minutes.ToString(), minute), ClockIcon, InactiveBrush, false);
    }

    private void TryAutosave()
    {
        if (Document.UpdateableChangeActive)
        {
            saveAfterNextFinish = true;
            
            savingTimer.Stop();
            updateTextTimer.Stop();
            
            UpdateMainMenuTextSave("AUTOSAVE_WAITING_FOR_SAVE", SaveIcon, ActiveBrush, true);
            
            return;
        }

        if (Document.AllChangesSaved)
        {
            updateTextTimer.Stop();
            RestartTimers();
            UpdateMainMenuTextSave("AUTOSAVE_NOTHING_CHANGED", SaveIcon, InactiveBrush, false);
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

            UpdateMainMenuTextSave(new LocalizedString("AUTOSAVE_FAILED_RETRYING", AutosavePeriodMinutes.ToString("0"), minute), WarnIcon, WarnBrush, true);
        
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
        
        UpdateMainMenuTextSave("AUTOSAVE_SAVING", SavingIcon, ActiveBrush, true);

        string filePath;
        bool fileExists = true;

        if (Document.FullFilePath == null || !Document.FullFilePath.EndsWith(".pixi"))
        {
            filePath = Path.Join(Paths.PathToUnsavedFilesFolder, $"autosave-{tempGuid}.pixi");
            Directory.CreateDirectory(Directory.GetParent(filePath)!.FullName);
        }
        else
        {
            filePath = Document.FullFilePath;
            fileExists = File.Exists(filePath);
        }
        
        busyTimer.Start();
        var result = Exporter.TrySave(Document, filePath);

        if (result == SaveResult.Success && fileExists)
        {
            savingFailed = 0;
            UpdateMainMenuTextSave("AUTOSAVE_SAVED", SaveIcon, SuccessBrush, false);
            Document.MarkAsSaved();
            LastSavedPath = filePath;
        }
        else if (result is SaveResult.InvalidPath or SaveResult.SecurityError || !fileExists)
        {
            UpdateMainMenuTextSave("AUTOSAVE_PLEASE_RESAVE", SaveIcon, ErrorBrush, true);
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
            
            UpdateMainMenuTextSave(new LocalizedString("AUTOSAVE_FAILED_RETRYING", AutosavePeriodMinutes.ToString("0"), minute), WarnIcon, WarnBrush, true);
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

    private void AutosavePeriodChanged(double minutes, bool documentEnabled)
    {
        if ((int)minutes == -1 || !documentEnabled)
        {
            savingTimer.Enabled = false;
            updateTextTimer.Enabled = false;
            saveAfterNextFinish = false;

            LocalizedString menuText = documentEnabled ? string.Empty : "AUTOSAVE_DISABLED";
            string iconText = documentEnabled ? null : PauseIcon;
            
            UpdateMainMenuTextSave(menuText, iconText, ActiveBrush, false);

            AutosavePeriodMinutes = minutes;
            return;
        }
        
        var timerEnabled = savingTimer.Enabled;

        if ((int)AutosavePeriodMinutes == -1 || !Enabled)
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

    private void UpdateMainMenuTextSave(LocalizedString text, string iconText, Brush brush, bool pulse)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            MainMenuText = text;
            MainMenuIconText = iconText;
            MainMenuBrush = brush;
            MainMenuPulse = pulse;
        });
    }
}
