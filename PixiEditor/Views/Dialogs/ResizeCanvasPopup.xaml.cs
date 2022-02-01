using PixiEditor.Models.Enums;
using System.Windows;
using System.Windows.Input;

namespace PixiEditor.Views
{
    /// <summary>
    ///     Interaction logic for ResizeCanvasPopup.xaml
    /// </summary>
    public partial class ResizeCanvasPopup : ResizeablePopup
    {
        // Using a DependencyProperty as the backing store for SelectedAnchorPoint.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedAnchorPointProperty =
            DependencyProperty.Register("SelectedAnchorPoint", typeof(AnchorPoint), typeof(ResizeCanvasPopup),
                new PropertyMetadata(AnchorPoint.Top | AnchorPoint.Left));

        
        public ResizeCanvasPopup()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            Loaded += (_, _) => sizePicker.FocusWidthPicker();
        }

        public AnchorPoint SelectedAnchorPoint
        {
            get => (AnchorPoint)GetValue(SelectedAnchorPointProperty);
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
}
