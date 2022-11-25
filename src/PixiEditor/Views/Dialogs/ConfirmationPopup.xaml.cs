using System.Windows;
using PixiEditor.Helpers;

namespace PixiEditor.Views.Dialogs;

/// <summary>
///     Interaction logic for ConfirmationPopup.xaml
/// </summary>
internal partial class ConfirmationPopup : Window
{
    public static readonly DependencyProperty ResultProperty =
        DependencyProperty.Register(nameof(Result), typeof(bool), typeof(ConfirmationPopup),
            new PropertyMetadata(true));

    public static readonly DependencyProperty BodyProperty =
        DependencyProperty.Register(nameof(Body), typeof(string), typeof(ConfirmationPopup), new PropertyMetadata(""));

    public static readonly DependencyProperty FirstOptionTextProperty = DependencyProperty.Register(
        nameof(FirstOptionText), typeof(string), typeof(ConfirmationPopup), new PropertyMetadata("Yes"));

    public string FirstOptionText
    {
        get { return (string)GetValue(FirstOptionTextProperty); }
        set { SetValue(FirstOptionTextProperty, value); }
    }

    public static readonly DependencyProperty SecondOptionTextProperty = DependencyProperty.Register(
        nameof(SecondOptionText), typeof(string), typeof(ConfirmationPopup), new PropertyMetadata("No"));

    public string SecondOptionText
    {
        get { return (string)GetValue(SecondOptionTextProperty); }
        set { SetValue(SecondOptionTextProperty, value); }
    }
    
    public ConfirmationPopup()
    {
        InitializeComponent();
        Owner = Application.Current.MainWindow;
        CancelCommand = new RelayCommand(Cancel);
        SetResultAndCloseCommand = new RelayCommand(SetResultAndClose);
        DataContext = this;
    }

    public RelayCommand CancelCommand { get; set; }
    public RelayCommand SetResultAndCloseCommand { get; set; }

    public bool Result
    {
        get => (bool)GetValue(ResultProperty);
        set => SetValue(ResultProperty, value);
    }


    public string Body
    {
        get => (string)GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    private void SetResultAndClose(object property)
    {
        bool result = (bool)property;
        Result = result;
        DialogResult = true;
        Close();
    }

    private void Cancel(object property)
    {
        DialogResult = false;
        Close();
    }
}
