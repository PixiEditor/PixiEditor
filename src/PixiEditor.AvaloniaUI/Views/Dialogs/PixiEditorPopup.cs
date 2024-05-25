using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.Extensions.CommonApi;
using PixiEditor.Extensions.CommonApi.Async;
using PixiEditor.Extensions.CommonApi.Windowing;

namespace PixiEditor.AvaloniaUI.Views.Dialogs;

public partial class PixiEditorPopup : Window, IPopupWindow
{
    public string UniqueId => "PixiEditor.Popup";

    public static readonly StyledProperty<bool> CanMinimizeProperty = AvaloniaProperty.Register<PixiEditorPopup, bool>(
        nameof(CanMinimize), defaultValue: true);

    public static readonly StyledProperty<bool> CloseIsHideProperty = AvaloniaProperty.Register<PixiEditorPopup, bool>(
        nameof(CloseIsHide), defaultValue: false);

    public static readonly StyledProperty<ICommand> CloseCommandProperty = AvaloniaProperty.Register<PixiEditorPopup, ICommand>(
        nameof(CloseCommand));

    public ICommand CloseCommand
    {
        get => GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }

    public bool CloseIsHide
    {
        get => GetValue(CloseIsHideProperty);
        set => SetValue(CloseIsHideProperty, value);
    }

    public bool CanMinimize
    {
        get => GetValue(CanMinimizeProperty);
        set => SetValue(CanMinimizeProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(PixiEditorPopup);

    public PixiEditorPopup()
    {
        CloseCommand = new RelayCommand(ClosePopup);
    }

    public override void Show()
    {
        Show(MainWindow.Current);
    }

    public AsyncCall<bool?> ShowDialog()
    {
        return AsyncCall<bool?>.FromTask(ShowDialog<bool?>(MainWindow.Current));
    }

    [RelayCommand]
    public void SetResultAndCloseCommand()
    {
        if(CloseIsHide)
            Hide();
        else
            Close(true);
    }

    public void ClosePopup()
    {
        if(CloseIsHide)
            Hide();
        else
            Close(false);
    }
}
