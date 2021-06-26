using PixiEditor.Models.Tools.Tools;
using PixiEditor.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xceed.Wpf.Toolkit.Core.Input;
using Xceed.Wpf.Toolkit.Zoombox;

namespace PixiEditor.Views
{
    public partial class MainDrawingPanel : UserControl
    {
        public static readonly DependencyProperty MouseXProperty =
            DependencyProperty.Register(nameof(MouseX), typeof(double), typeof(MainDrawingPanel), new PropertyMetadata(null));

        public static readonly DependencyProperty MouseYProperty =
            DependencyProperty.Register(nameof(MouseY), typeof(double), typeof(MainDrawingPanel), new PropertyMetadata(null));

        public static readonly DependencyProperty MouseMoveCommandProperty =
            DependencyProperty.Register(nameof(MouseMoveCommand), typeof(ICommand), typeof(MainDrawingPanel),
                new PropertyMetadata(null));

        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register(nameof(Item), typeof(object), typeof(MainDrawingPanel), new PropertyMetadata(default(FrameworkElement)));

        public static readonly DependencyProperty IsUsingZoomToolProperty =
            DependencyProperty.Register(nameof(IsUsingZoomTool), typeof(bool), typeof(MainDrawingPanel), new PropertyMetadata(false));

        public double MouseX
        {
            get => (double)GetValue(MouseXProperty);
            set => SetValue(MouseXProperty, value);
        }

        public double MouseY
        {
            get => (double)GetValue(MouseYProperty);
            set => SetValue(MouseYProperty, value);
        }

        public ICommand MouseMoveCommand
        {
            get => (ICommand)GetValue(MouseMoveCommandProperty);
            set => SetValue(MouseMoveCommandProperty, value);
        }

        public object Item
        {
            get => GetValue(ItemProperty);
            set => SetValue(ItemProperty, value);
        }

        public bool IsUsingZoomTool
        {
            get => (bool)GetValue(IsUsingZoomToolProperty);
            set => SetValue(IsUsingZoomToolProperty, value);
        }

        public Point ClickPosition;

        public MainDrawingPanel()
        {
            InitializeComponent();
            Zoombox.ZoomToSelectionModifiers = new KeyModifierCollection() { KeyModifier.RightAlt };
        }

        private void MainDrawingPanel_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            IsUsingZoomTool = ViewModelMain.Current.BitmapManager.SelectedTool is ZoomTool;
            Mouse.Capture((IInputElement)sender, CaptureMode.SubTree);
            ClickPosition = ((FrameworkElement)Item).TranslatePoint(new Point(0, 0), Zoombox);
        }

        private void MainDrawingPanel_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            ((IInputElement)sender).ReleaseMouseCapture();
        }

        private void Zoombox_CurrentViewChanged(object sender, ZoomboxViewChangedEventArgs e)
        {
            Zoombox.MinScale = 32 / ((FrameworkElement)Item).Width;
        }
    }
}
