using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Input;

namespace PixiEditor.AvaloniaUI.Views.Dialogs;

public partial class PixiEditorPopup : Window, IStyleable
{
    Type IStyleable.StyleKey => typeof(PixiEditorPopup);

    [RelayCommand]
    public void SetResultAndCloseCommand()
    {
        Close(true);
    }

    [RelayCommand]
    public void CloseCommand()
    {
        Close(false);
    }
}
