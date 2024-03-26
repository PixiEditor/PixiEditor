using System.Windows;
using PixiEditor.Extensions;
using PixiEditor.Extensions.Windowing;
using PixiEditor.Helpers;

namespace PixiEditor.Views.Dialogs;

internal partial class BasicPopup : Window, IPopupWindow
{
    public string UniqueId => "PixiEditor.EmptyPopup";

    public RelayCommand CancelCommand { get; set; }

    public static readonly DependencyProperty BodyProperty = DependencyProperty.Register(
        nameof(Body), typeof(object), typeof(BasicPopup), new PropertyMetadata(default(object)));

    public object Body
    {
        get { return (object)GetValue(BodyProperty); }
        set { SetValue(BodyProperty, value); }
    }

    public BasicPopup()
    {
        InitializeComponent();
        CancelCommand = new RelayCommand(Cancel);
        DataContext = this;
    }

    private void Cancel(object obj)
    {
        if (this.IsModal())
        {
            DialogResult = false;
        }

        Close();
    }
}

