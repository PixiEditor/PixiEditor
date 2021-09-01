using PixiEditor.Helpers;
using PixiEditor.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PixiEditor.Views
{
    /// <summary>
    /// Interaction logic for ToolSettingColorPicker.xaml.
    /// </summary>
    public partial class ToolSettingColorPicker : UserControl
    {
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(ToolSettingColorPicker));

        public Color SelectedColor
        {
            get => (Color)GetValue(SelectedColorProperty);
            set
            {
                SetValue(SelectedColorProperty, value);
            }
        }

        public static readonly DependencyProperty CopyMainColorCommandProperty = DependencyProperty.Register(
            nameof(CopyMainColorCommand), typeof(RelayCommand), typeof(ToolSettingColorPicker));

        public RelayCommand CopyMainColorCommand
        {
            get { return (RelayCommand)GetValue(CopyMainColorCommandProperty); }
            set { SetValue(CopyMainColorCommandProperty, value); }
        }

        public ToolSettingColorPicker()
        {
            InitializeComponent();
            ColorPicker.SecondaryColor = Colors.Black;

            CopyMainColorCommand = new RelayCommand(CopyMainColor);
        }

        public void CopyMainColor(object parameter)
        {
            SelectedColor = Color.FromArgb(
                ViewModelMain.Current.ColorsSubViewModel.PrimaryColor.Alpha,
                ViewModelMain.Current.ColorsSubViewModel.PrimaryColor.Red,
                ViewModelMain.Current.ColorsSubViewModel.PrimaryColor.Green,
                ViewModelMain.Current.ColorsSubViewModel.PrimaryColor.Blue);
        }
    }
}
