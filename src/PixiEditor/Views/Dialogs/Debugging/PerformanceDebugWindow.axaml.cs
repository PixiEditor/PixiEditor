using PixiEditor.Models.AnalyticsAPI;

namespace PixiEditor.Views.Dialogs.Debugging;

public partial class PerformanceDebugWindow : PixiEditorPopup
{
    public StartupPerformance StartupPerformance
    {
        get => MainWindow.Current.StartupPerformance;
    }
    
    public PerformanceDebugWindow()
    {
        InitializeComponent();
    }
}

