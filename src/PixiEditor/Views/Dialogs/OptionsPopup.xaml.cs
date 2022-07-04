using PixiEditor.Helpers;
using System.Collections.ObjectModel;
using System.Windows;

namespace PixiEditor.Views.Dialogs;

public partial class OptionPopup : Window
{
    public static readonly DependencyProperty PopupContentProperty =
        DependencyProperty.Register(nameof(PopupContent), typeof(object), typeof(OptionPopup));

    public object PopupContent
    {
        get => GetValue(PopupContentProperty);
        set => SetValue(PopupContentProperty, value);
    }

    public static readonly DependencyProperty OptionsProperty =
        DependencyProperty.Register(nameof(Options), typeof(ObservableCollection<object>), typeof(OptionPopup));

    public ObservableCollection<object> Options
    {
        get => (ObservableCollection<object>)GetValue(OptionsProperty);
        set => SetValue(OptionsProperty, value);
    }

    public static readonly DependencyProperty ResultProperty =
        DependencyProperty.Register(nameof(Result), typeof(object), typeof(OptionPopup));

    public object Result
    {
        get => GetValue(ResultProperty);
        set => SetValue(ResultProperty, value);
    }

    public RelayCommand CancelCommand { get; set; }

    public RelayCommand CloseCommand { get; set; }

    public OptionPopup(string title, object content, ObservableCollection<object> options)
    {
        Title = title;
        PopupContent = content;
        Options = options;
        CancelCommand = new RelayCommand(Cancel);
        CloseCommand = new RelayCommand(Close);
        InitializeComponent();
        ContentRendered += OptionPopup_ContentRendered;
    }

    private void OptionPopup_ContentRendered(object sender, EventArgs e)
    {
        InvalidateVisual();
    }

    private void Cancel(object arg)
    {
        DialogResult = false;
        Close();
    }

    private void Close(object parameter)
    {
        DialogResult = true;
        Result = parameter;
        Close();
    }
}