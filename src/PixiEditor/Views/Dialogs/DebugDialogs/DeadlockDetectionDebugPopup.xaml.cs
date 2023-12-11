using System.Windows;
using System.Windows.Input;
using PixiEditor.Helpers;

namespace PixiEditor.Views.Dialogs.DebugDialogs;

public partial class DeadlockDetectionDebugPopup : Window
{
    public static readonly DependencyProperty TimeSinceStartProperty = DependencyProperty.Register(
        nameof(TimeSinceStart), typeof(string), typeof(DeadlockDetectionDebugPopup), new PropertyMetadata(default(string)));

    public string TimeSinceStart
    {
        get { return (string)GetValue(TimeSinceStartProperty); }
        set { SetValue(TimeSinceStartProperty, value); }
    }
    
    public static readonly DependencyProperty TotalChecksProperty = DependencyProperty.Register(
        nameof(TotalChecks), typeof(int), typeof(DeadlockDetectionDebugPopup), new PropertyMetadata(default(int)));

    public int TotalChecks
    {
        get => (int)GetValue(TotalChecksProperty);
        set => SetValue(TotalChecksProperty, value);
    }

    public static readonly DependencyProperty SecondStageChecksProperty = DependencyProperty.Register(
        nameof(SecondStageChecks), typeof(int), typeof(DeadlockDetectionDebugPopup), new PropertyMetadata(default(int)));

    public int SecondStageChecks
    {
        get => (int)GetValue(SecondStageChecksProperty);
        set => SetValue(SecondStageChecksProperty, value);
    }

    public static readonly DependencyProperty ThirdStageChecksProperty = DependencyProperty.Register(
        nameof(ThirdStageChecks), typeof(int), typeof(DeadlockDetectionDebugPopup), new PropertyMetadata(default(int)));

    public int ThirdStageChecks
    {
        get => (int)GetValue(ThirdStageChecksProperty);
        set => SetValue(ThirdStageChecksProperty, value);
    }

    public static readonly DependencyProperty FourthStageChecksProperty = DependencyProperty.Register(
        nameof(FourthStageChecks), typeof(int), typeof(DeadlockDetectionDebugPopup), new PropertyMetadata(default(int)));

    public int FourthStageChecks
    {
        get => (int)GetValue(FourthStageChecksProperty);
        set => SetValue(FourthStageChecksProperty, value);
    }

    public static readonly DependencyProperty DeadlocksDetectedProperty = DependencyProperty.Register(
        nameof(DeadlocksDetected), typeof(int), typeof(DeadlockDetectionDebugPopup), new PropertyMetadata(default(int)));

    public int DeadlocksDetected
    {
        get { return (int)GetValue(DeadlocksDetectedProperty); }
        set { SetValue(DeadlocksDetectedProperty, value); }
    }

    private DeadlockDetectionHelper helper;
    
    public DeadlockDetectionDebugPopup()
    {
        //helper = DeadlockDetectionHelper.Current;
        DataContext = this;
        InitializeComponent();
        Refresh_OnClick(null, null);
    }
    
    private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
    {
        SystemCommands.CloseWindow(this);
    }

    private void Refresh_OnClick(object sender, RoutedEventArgs e)
    {
        TimeSinceStart = (DateTime.Now - helper.StartTime).ToString();
        TotalChecks = helper.TotalChecks;
        SecondStageChecks = helper.SecondStageChecks;
        ThirdStageChecks = helper.ThirdStageChecks;
        FourthStageChecks = helper.FourthStageChecks;
        DeadlocksDetected = helper.DeadlocksDetected;
    }
}

