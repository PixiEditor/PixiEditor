using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.DocumentModels.Autosave.Enums;
using PixiEditor.Models.DocumentModels.Autosave.Structs;

namespace PixiEditor.Views.UserControls;

public partial class AutosaveControl : UserControl, INotifyPropertyChanged
{
    public static readonly DependencyProperty AutosaveStateDataProperty =
        DependencyProperty.Register(nameof(AutosaveStateData), typeof(AutosaveStateData?), typeof(AutosaveControl), new PropertyMetadata(OnStateChanged));
    
    public AutosaveStateData? AutosaveStateData
    {
        get => (AutosaveStateData?)GetValue(AutosaveStateDataProperty);
        set => SetValue(AutosaveStateDataProperty, value);
    }
    
    public static readonly DependencyProperty AutosaveEnabledProperty =
        DependencyProperty.Register(nameof(AutosaveEnabled), typeof(bool), typeof(AutosaveControl));
    
    public bool AutosaveEnabled
    {
        get => (bool)GetValue(AutosaveEnabledProperty);
        set => SetValue(AutosaveEnabledProperty, value);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    
    private const string ClockIcon = "\ue84d";
    private const string WarnIcon = "\ue81e";
    private const string SaveIcon = "\ue8bc";
    private const string PauseIcon = "\ue8a2";
    
    private const double TimerIntervalSeconds = 3.8;

    private readonly Brush errorBrush = new SolidColorBrush(Color.FromArgb(255, 214, 66, 56));
    private readonly Brush warnBrush = new SolidColorBrush(Color.FromArgb(255, 219, 189, 53));
    private readonly Brush successBrush = new SolidColorBrush(Color.FromArgb(255, 83, 207, 72));
    private readonly Brush activeBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
    private readonly Brush inactiveBrush = new SolidColorBrush(Color.FromArgb(255, 120, 120, 120));
    
    private DispatcherTimer textUpdateTimer;

    private string iconText;

    public string IconText
    {
        get => iconText;
        set => SetField(ref iconText, value);
    }

    private string text;

    public LocalizedString Text
    {
        get => text;
        set => SetField(ref text, value);
    }

    private bool isForceExpanded;

    public bool IsForceExpanded
    {
        get => isForceExpanded;
        set => SetField(ref isForceExpanded, value);
    }

    private Brush iconBrush;

    public Brush IconBrush
    {
        get => iconBrush;
        set => SetField(ref iconBrush, value);
    }

    private bool pulseIcon;

    public bool PulseIcon
    {
        get => pulseIcon;
        set => SetField(ref pulseIcon, value);
    }
    
    public AutosaveControl()
    {
        InitializeComponent();

        textUpdateTimer = new DispatcherTimer(TimeSpan.FromSeconds(TimerIntervalSeconds), DispatcherPriority.Normal, (_, _) => Update(), Application.Current.Dispatcher)
        {
            IsEnabled = true
        };
    }

    private void Update()
    {
        if (AutosaveStateData is null || AutosaveStateData.Value.AutosaveState is AutosaveState.Paused )
        {
            UpdateTextSave("AUTOSAVE_DISABLED", false, PauseIcon, activeBrush, false);
            textUpdateTimer.Stop();
            return;
        }
        if (!textUpdateTimer.IsEnabled)
            textUpdateTimer.Start();
        
        var data = AutosaveStateData.Value;
        if (data.LastBackupAutosaveData is null)
        {
            SetWaitingToSave(data);
            return;
        }
        
        if (data.AutosaveState is AutosaveState.Idle)
        {
            if ((DateTime.Now - data.LastBackupAutosaveData.Value.Time).TotalSeconds < (TimerIntervalSeconds - 0.1))
            {
                // just autosaved, show result
                bool showingError = false;
                if (data.LastUserFileAutosaveData?.SaveResult is UserFileAutosaveResult.NoUserFile)
                {
                    UpdateTextSave("AUTOSAVE_PLEASE_RESAVE", true, SaveIcon, errorBrush, true);
                    showingError = true;
                }
                if (data.LastBackupAutosaveData.Value.SaveResult is BackupAutosaveResult.Error || 
                    data.LastUserFileAutosaveData?.SaveResult is UserFileAutosaveResult.ExceptionWhileSaving)
                {
                    SetWaitingToSave(data);
                    showingError = true;
                }
                if (showingError)
                    return;

                if (data.LastBackupAutosaveData.Value.SaveResult is BackupAutosaveResult.NothingToSave)
                {
                    UpdateTextSave("AUTOSAVE_NOTHING_CHANGED", false, SaveIcon, inactiveBrush, false);
                    return;
                }

                if (data.LastBackupAutosaveData.Value.SaveResult is BackupAutosaveResult.Success)
                {
                    UpdateTextSave("AUTOSAVE_SAVED", true, SaveIcon, successBrush, false);
                    return;
                }
            }
            else
            {
                SetWaitingToSave(data);
                return;
            }
        }
        
        if (data.AutosaveState is AutosaveState.AwaitingUpdateableChangeEnd)
        {
            UpdateTextSave("AUTOSAVE_WAITING_FOR_SAVE", true, SaveIcon, activeBrush, true);
            return;
        }

        if (data.AutosaveState is AutosaveState.InProgress)
        {
            UpdateTextSave("AUTOSAVE_SAVING", true, SaveIcon, activeBrush, true);
            return;
        }
    }

    private void SetWaitingToSave(AutosaveStateData data)
    {
        TimeSpan timeLeft = data.LastBackupAutosaveData switch
        {
            null => data.AutosaveInterval - (DateTime.Now - data.AutosaveLaunchDateTime),
            { } lastData => data.AutosaveInterval - (DateTime.Now - lastData.Time)
        };
        
        bool error = (data.LastBackupAutosaveData, data.LastUserFileAutosaveData) switch
        {
            (null, null) => false,
            ({ } backup, null) => backup.SaveResult == BackupAutosaveResult.Error,
            ({ } backup, { } autosave) => backup.SaveResult == BackupAutosaveResult.Error || autosave.SaveResult == UserFileAutosaveResult.ExceptionWhileSaving
        };
        
        if (timeLeft.Minutes == 0 && !error)
        {
            UpdateTextSave("AUTOSAVE_SAVING_IN_MINUTE", false, ClockIcon, inactiveBrush, false);
            return;
        }
        
        TimeSpan adjusted = timeLeft.Add(TimeSpan.FromSeconds(30));
        LocalizedString minute = adjusted.Minutes < 2
            ? new LocalizedString("MINUTE_SINGULAR")
            : new LocalizedString("MINUTE_PLURAL");
        
        if (error)
            UpdateTextSave(new LocalizedString("AUTOSAVE_FAILED_RETRYING", data.AutosaveInterval.TotalMinutes.ToString("0"), minute), true, WarnIcon, warnBrush, true);
        else
            UpdateTextSave(new LocalizedString("AUTOSAVE_SAVING_IN", adjusted.Minutes.ToString(), minute), false, ClockIcon, inactiveBrush, false);
    }
    
    private void UpdateTextSave(LocalizedString text, bool isImportantText, string iconText, Brush brush, bool pulse)
    {
        Text = text;
        isForceExpanded = isImportantText;
        IconText = iconText;
        IconBrush = brush;
        PulseIcon = pulse;
    }
    
    private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var self = (AutosaveControl)d;
        self.Update();
        self.textUpdateTimer.Stop();
        self.textUpdateTimer.Start();
    }
    
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
