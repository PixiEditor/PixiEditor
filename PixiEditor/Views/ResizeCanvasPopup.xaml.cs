using System.Windows;
using System.Windows.Input;
using PixiEditor.Models.Enums;

namespace PixiEditor.Views
{
    /// <summary>
    ///     Interaction logic for ResizeCanvasPopup.xaml
    /// </summary>
    public partial class ResizeCanvasPopup : Window
    {
        // Using a DependencyProperty as the backing store for SelectedAnchorPoint.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedAnchorPointProperty =
            DependencyProperty.Register("SelectedAnchorPoint", typeof(AnchorPoint), typeof(ResizeCanvasPopup),
                new PropertyMetadata(AnchorPoint.Top | AnchorPoint.Left));

        // Using a DependencyProperty as the backing store for NewHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NewHeightProperty =
            DependencyProperty.Register("NewHeight", typeof(int), typeof(ResizeCanvasPopup), new PropertyMetadata(0));

        // Using a DependencyProperty as the backing store for NewWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NewWidthProperty =
            DependencyProperty.Register("NewWidth", typeof(int), typeof(ResizeCanvasPopup), new PropertyMetadata(0));

        public ResizeCanvasPopup()
        {
            InitializeComponent();
        }


        public AnchorPoint SelectedAnchorPoint
        {
            get => (AnchorPoint) GetValue(SelectedAnchorPointProperty);
            set => SetValue(SelectedAnchorPointProperty, value);
        }


        public int NewHeight
        {
            get => (int) GetValue(NewHeightProperty);
            set => SetValue(NewHeightProperty, value);
        }


        public int NewWidth
        {
            get => (int) GetValue(NewWidthProperty);
            set => SetValue(NewWidthProperty, value);
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