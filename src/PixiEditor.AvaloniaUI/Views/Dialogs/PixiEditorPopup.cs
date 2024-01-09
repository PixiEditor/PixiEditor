using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.AvaloniaUI.Helpers.Extensions;

namespace PixiEditor.AvaloniaUI.Views.Dialogs;

public partial class PixiEditorPopup : Window, IStyleable
{
    public static readonly StyledProperty<bool> CanMinimizeProperty = AvaloniaProperty.Register<PixiEditorPopup, bool>(
        nameof(CanMinimize), defaultValue: true);

    public bool CanMinimize
    {
        get => GetValue(CanMinimizeProperty);
        set => SetValue(CanMinimizeProperty, value);
    }

    Type IStyleable.StyleKey => typeof(PixiEditorPopup);

    public override void Show()
    {
        Show(MainWindow.Current);
    }

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
