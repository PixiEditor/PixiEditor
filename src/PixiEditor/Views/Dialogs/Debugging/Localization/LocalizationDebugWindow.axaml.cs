using Avalonia.Controls;

namespace PixiEditor.Views.Dialogs.Debugging.Localization;

public partial class LocalizationDebugWindow : PixiEditorPopup
{
    private static LocalizationDataContext dataContext;
    private bool passedStartup;

    public LocalizationDebugWindow()
    {
        InitializeComponent();
        DataContext = (dataContext ??= new LocalizationDataContext());
    }

    private void ApiKeyChanged(object sender, TextChangedEventArgs e)
    {
        if (!passedStartup)
        {
            passedStartup = true;
            return;
        }

        dataContext.LoggedIn = false;
        dataContext.StatusMessage = "NOT_LOGGED_IN"; // TODO: For some reason it was NOT_LOGGED_IN
    }
}

