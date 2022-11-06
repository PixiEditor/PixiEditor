using System.Windows;
using System.Windows.Input;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.Views.Dialogs;

/// <summary>
///     Interaction logic for ResizeCanvasPopup.xaml
/// </summary>
internal partial class ResizeCanvasPopup : ResizeablePopup
{

    public static readonly DependencyProperty SelectedAnchorPointProperty =
        DependencyProperty.Register(nameof(SelectedAnchorPoint), typeof(ResizeAnchor), typeof(ResizeCanvasPopup),
            new PropertyMetadata(ResizeAnchor.TopLeft));


    public ResizeCanvasPopup()
    {
        InitializeComponent();
        Owner = Application.Current.MainWindow;
        Loaded += (_, _) => sizePicker.FocusWidthPicker();
    }

    public ResizeAnchor SelectedAnchorPoint
    {
        get => (ResizeAnchor)GetValue(SelectedAnchorPointProperty);
        set => SetValue(SelectedAnchorPointProperty, value);
    }

    private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
    {
        SystemCommands.CloseWindow(this);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}
