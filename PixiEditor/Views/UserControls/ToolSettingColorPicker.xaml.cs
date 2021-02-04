using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using ColorPicker;
using PixiEditor.Helpers;
using PixiEditor.ViewModels;

namespace PixiEditor.Views
{
    /// <summary>
    /// Interaction logic for ToolSettingColorPicker.xaml.
    /// </summary>
    public partial class ToolSettingColorPicker : UserControl
    {
        public static DependencyProperty SelectedColorProperty =
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

            CopyMainColorCommand = new RelayCommand(CopyMainColor);
        }

        public void CopyMainColor(object parameter)
        {
            SelectedColor = ViewModelMain.Current.ColorsSubViewModel.PrimaryColor;
        }
    }
}