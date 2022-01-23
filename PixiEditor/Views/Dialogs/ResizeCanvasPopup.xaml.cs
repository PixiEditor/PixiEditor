using PixiEditor.Models.Enums;
using System.Windows;
using System.Windows.Input;

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
        public static readonly DependencyProperty NewAbsoluteHeightProperty =
            DependencyProperty.Register("NewAbsoluteHeight", typeof(int), typeof(ResizeCanvasPopup), new PropertyMetadata(0));

        // Using a DependencyProperty as the backing store for NewWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NewAbsoluteWidthProperty =
            DependencyProperty.Register("NewAbsoluteWidth", typeof(int), typeof(ResizeCanvasPopup), new PropertyMetadata(0));

        public static readonly DependencyProperty NewPercentageSizeProperty =
            DependencyProperty.Register("NewPercentageSize", typeof(int), typeof(ResizeCanvasPopup), new PropertyMetadata(0));

        public static readonly DependencyProperty NewSelectedUnitProperty =
            DependencyProperty.Register(nameof(NewSelectedUnit), typeof(SizeUnit), typeof(SizePicker), new PropertyMetadata(SizeUnit.Pixel));

        public ResizeCanvasPopup()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
        }


        public AnchorPoint SelectedAnchorPoint
        {
            get => (AnchorPoint)GetValue(SelectedAnchorPointProperty);
            set => SetValue(SelectedAnchorPointProperty, value);
        }


        public int NewAbsoluteHeight
        {
            get => (int)GetValue(NewAbsoluteHeightProperty);
            set => SetValue(NewAbsoluteHeightProperty, value);
        }

        public int NewAbsoluteWidth
        {
            get => (int)GetValue(NewAbsoluteWidthProperty);
            set => SetValue(NewAbsoluteWidthProperty, value);
        }

        public int NewPercentageSize 
        {
            get => (int)GetValue(NewPercentageSizeProperty);
            set => SetValue(NewPercentageSizeProperty, value);
        }

        public SizeUnit NewSelectedUnit
        {
            get => (SizeUnit)GetValue(NewSelectedUnitProperty);
            set => SetValue(NewSelectedUnitProperty, value);
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
