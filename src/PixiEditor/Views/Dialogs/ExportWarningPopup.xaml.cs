using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PixiEditor.Views.Dialogs;
/// <summary>
/// Interaktionslogik für ExportWarningPopup.xaml
/// </summary>
internal partial class ExportWarningPopup : Window
{
    public static readonly DependencyProperty BodyProperty =
        DependencyProperty.Register(nameof(Body), typeof(string), typeof(ExportWarningPopup));

    public new string Title
    {
        get => base.Title;
        set => base.Title = value;
    }

    public string Body
    {
        get => (string)GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    public ExportWarningPopup()
    {
        InitializeComponent();
    }

    private void OkButton_Close(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
