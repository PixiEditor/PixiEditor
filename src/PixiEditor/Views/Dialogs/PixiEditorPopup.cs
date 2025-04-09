using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using PixiDocks.Avalonia.Helpers;
using PixiEditor.Extensions.CommonApi;
using PixiEditor.Extensions.CommonApi.Async;
using PixiEditor.Extensions.CommonApi.Windowing;
using PixiEditor.Extensions.UI;

namespace PixiEditor.Views.Dialogs;

[TemplatePart("PART_ResizePanel", typeof(Panel))]
[TemplatePart("Part_TitleBar", typeof(DialogTitleBar))]
public partial class PixiEditorPopup : Window, IPopupWindow
{
    public string UniqueId => "PixiEditor.Popup";

    public static readonly StyledProperty<bool> CanMinimizeProperty = AvaloniaProperty.Register<PixiEditorPopup, bool>(
        nameof(CanMinimize), defaultValue: true);

    public static readonly StyledProperty<bool> CloseIsHideProperty = AvaloniaProperty.Register<PixiEditorPopup, bool>(
        nameof(CloseIsHide), defaultValue: false);

    public static readonly StyledProperty<ICommand> CloseCommandProperty =
        AvaloniaProperty.Register<PixiEditorPopup, ICommand>(
            nameof(CloseCommand));

    public static readonly StyledProperty<bool> ShowTitleBarProperty = AvaloniaProperty.Register<PixiEditorPopup, bool>(
        nameof(ShowTitleBar), defaultValue: true);

    public bool ShowTitleBar
    {
        get => GetValue(ShowTitleBarProperty);
        set => SetValue(ShowTitleBarProperty, value);
    }

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

    private Panel resizePanel;

    protected override Type StyleKeyOverride => typeof(PixiEditorPopup);

    public PixiEditorPopup()
    {
        CloseCommand = new RelayCommand(ClosePopup);
#if DEBUG
        this.AttachDevTools();
#endif
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (System.OperatingSystem.IsLinux())
        {
            var titleBar = e.NameScope.Find<DialogTitleBar>("PART_TitleBar");
            titleBar.PointerPressed += OnTitleBarPressed;

            resizePanel = e.NameScope.Find<Panel>("PART_ResizePanel");
            resizePanel.AddHandler(PointerPressedEvent, OnResizePanelPressed,
                RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
            resizePanel.PointerMoved += OnResizePanelMoved;
        }
    }

    private void OnTitleBarPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            if (e.ClickCount == 2)
            {
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            }
            else
            {
                BeginMoveDrag(e);
                e.Handled = true;
            }
        }
    }

    private void OnResizePanelPressed(object? sender, PointerPressedEventArgs e)
    {
        if (WindowState == WindowState.Normal && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && CanResize)
        {
            var dir = WindowUtility.GetResizeDirection(e.GetPosition(resizePanel), resizePanel, new Thickness(8));
            if (dir == null) return;

            BeginResizeDrag(dir.Value, e);
            e.Handled = true;
        }
    }

    private void OnResizePanelMoved(object? sender, PointerEventArgs e)
    {
        if (!CanResize || WindowState != WindowState.Normal) return;
        Cursor = new Cursor(WindowUtility.SetResizeCursor(e, resizePanel, new Thickness(8)));
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
        if (CloseIsHide)
            Hide();
        else
            Close(true);
    }

    public void ClosePopup()
    {
        if (CloseIsHide)
            Hide();
        else
            Close(false);
    }
}
