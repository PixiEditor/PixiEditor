using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using PixiEditor.Extensions.Common.Localization;

namespace PixiEditor.Views.UserControls;

public enum AutosaveState
{
    Paused,
    Idle,
    AwaitingUpdateableChangeEnd,
    InProgress
}

public enum UserFileAutosaveResult
{
    Success,
    NoUserFile,
    ExceptionWhileSaving,
    Disabled,
    NothingToSave
}

public enum BackupAutosaveResult
{
    Success,
    Error,
    NothingToSave
}

public struct LastAutosaveData
{
    public DateTime Time { get; set; }
    
    public UserFileAutosaveResult UserFileSaveResult { get; set; }
    
    public BackupAutosaveResult BackupSaveResult { get; set; }
}

public struct AutosaveStateData
{
    public LastAutosaveData? LastAutosaveData { get; set; }
    public AutosaveState AutosaveState { get; set; }
    public DateTime AutosaveLaunchDateTime { get; set; }
    public TimeSpan AutosaveInterval { get; set; }
}

public partial class AutosaveControl : UserControl, INotifyPropertyChanged
{
    public static readonly DependencyProperty AutosaveStateDataProperty =
        DependencyProperty.Register(nameof(AutosaveStateData), typeof(AutosaveStateData), typeof(AutosaveControl), new PropertyMetadata(OnStateChanged));
    
    public AutosaveStateData AutosaveStateData
    {
        get => (AutosaveStateData)GetValue(AutosaveStateDataProperty);
        set => SetValue(AutosaveStateDataProperty, value);
    }
    
    public static readonly DependencyProperty AutosaveEnabledProperty =
        DependencyProperty.Register(nameof(AutosaveEnabled), typeof(AutosaveStateData), typeof(AutosaveControl));
    
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

    private readonly Brush errorBrush = new SolidColorBrush(Color.FromArgb(255, 214, 66, 56));
    private readonly Brush warnBrush = new SolidColorBrush(Color.FromArgb(255, 219, 189, 53));
    private readonly Brush successBrush = new SolidColorBrush(Color.FromArgb(255, 83, 207, 72));
    private readonly Brush activeBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
    private readonly Brush inactiveBrush = new SolidColorBrush(Color.FromArgb(255, 120, 120, 120));
    
    private DispatcherTimer textUpdateTimer;
    private const double timerIntervalSeconds = 3.8;

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
        DataContext = this;

        textUpdateTimer = new DispatcherTimer(TimeSpan.FromSeconds(timerIntervalSeconds), DispatcherPriority.Normal, (_, _) => Update(), Application.Current.Dispatcher)
        {
            IsEnabled = true
        };
    }

    private void Update()
    {
        var data = AutosaveStateData;
        if (data.AutosaveState is AutosaveState.Paused)
        {
            UpdateTextSave("AUTOSAVE_DISABLED", false, PauseIcon, activeBrush, false);
            return;
        }
        
        if (data.LastAutosaveData is null)
        {
            SetWaitingToSave();
            return;
        }
        
        if (AutosaveStateData.AutosaveState is AutosaveState.Idle)
        {
            if ((DateTime.Now - data.LastAutosaveData.Value.Time).TotalSeconds < (timerIntervalSeconds - 0.1))
            {
                // just autosaved, show result
                bool showingError = false;
                if (data.LastAutosaveData.Value.UserFileSaveResult is UserFileAutosaveResult.NoUserFile)
                {
                    UpdateTextSave("AUTOSAVE_PLEASE_RESAVE", true, SaveIcon, errorBrush, true);
                    showingError = true;
                }
                if (data.LastAutosaveData.Value.BackupSaveResult is BackupAutosaveResult.Error || 
                    data.LastAutosaveData.Value.UserFileSaveResult is UserFileAutosaveResult.ExceptionWhileSaving)
                {
                    SetWaitingToSave();
                    showingError = true;
                }
                if (showingError)
                    return;

                if (data.LastAutosaveData.Value.BackupSaveResult is BackupAutosaveResult.NothingToSave)
                {
                    UpdateTextSave("AUTOSAVE_NOTHING_CHANGED", false, SaveIcon, inactiveBrush, false);
                    return;
                }

                if (data.LastAutosaveData.Value.BackupSaveResult is BackupAutosaveResult.Success)
                {
                    UpdateTextSave("AUTOSAVE_SAVED", true, SaveIcon, successBrush, false);
                    return;
                }
            }
            else
            {
                SetWaitingToSave();
                return;
            }
        }
        
        if (AutosaveStateData.AutosaveState is AutosaveState.AwaitingUpdateableChangeEnd)
        {
            UpdateTextSave("AUTOSAVE_WAITING_FOR_SAVE", true, SaveIcon, activeBrush, true);
            return;
        }

        if (AutosaveStateData.AutosaveState is AutosaveState.InProgress)
        {
            UpdateTextSave("AUTOSAVE_SAVING", true, SaveIcon, activeBrush, true);
            return;
        }
    }

    private void SetWaitingToSave()
    {
        var data = AutosaveStateData;
        TimeSpan timeLeft = data.LastAutosaveData switch
        {
            null => data.AutosaveInterval - (DateTime.Now - data.AutosaveLaunchDateTime),
            { } lastData => data.AutosaveInterval - (DateTime.Now - lastData.Time)
        };
        
        bool error = data.LastAutosaveData switch
        {
            null => false,
            { } lastData => lastData.BackupSaveResult != BackupAutosaveResult.Error && lastData.UserFileSaveResult != UserFileAutosaveResult.ExceptionWhileSaving
        };
        
        if (timeLeft.Minutes == 0 && !error)
        {
            UpdateTextSave("AUTOSAVE_SAVING_IN_MINUTE", false, ClockIcon, inactiveBrush, false);
            return;
        }
        
        var adjusted = timeLeft.Add(TimeSpan.FromSeconds(30));
        var minute = adjusted.Minutes < 2
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

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
