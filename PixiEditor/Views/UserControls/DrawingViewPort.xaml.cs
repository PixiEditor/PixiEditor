using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PixiEditor.Views.UserControls
{
    /// <summary>
    /// Interaction logic for DrawingViewPort.xaml.
    /// </summary>
    public partial class DrawingViewPort : UserControl
    {
        public DrawingViewPort()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ZoomPercentageProperty =
            DependencyProperty.Register("ZoomPercentage", typeof(float), typeof(DrawingViewPort), new PropertyMetadata(100f));

        public static readonly DependencyProperty RecenterZoomboxProperty =
            DependencyProperty.Register("RecenterZoombox", typeof(bool), typeof(DrawingViewPort), new PropertyMetadata(false));

        public static readonly DependencyProperty MiddleMouseClickedCommandProperty =
            DependencyProperty.Register("MiddleMouseClickedCommand", typeof(ICommand), typeof(DrawingViewPort), new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty ViewportPositionProperty =
            DependencyProperty.Register("ViewportPosition", typeof(Point), typeof(DrawingViewPort), new PropertyMetadata(default(Point)));

        public static readonly DependencyProperty MouseMoveCommandProperty =
            DependencyProperty.Register("MouseMoveCommand", typeof(ICommand), typeof(DrawingViewPort), new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty MouseDownCommandProperty =
            DependencyProperty.Register("MouseDownCommand", typeof(ICommand), typeof(DrawingViewPort), new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty MouseXOnCanvasProperty =
            DependencyProperty.Register("MouseXOnCanvas", typeof(double), typeof(DrawingViewPort), new PropertyMetadata(0.0));

        public static readonly DependencyProperty MouseYOnCanvasProperty =
            DependencyProperty.Register("MouseYOnCanvas", typeof(double), typeof(DrawingViewPort), new PropertyMetadata(0.0));

        public static readonly DependencyProperty GridLinesVisibleProperty =
            DependencyProperty.Register("GridLinesVisible", typeof(bool), typeof(DrawingViewPort), new PropertyMetadata(false));

        public float ZoomPercentage
        {
            get => (float)GetValue(ZoomPercentageProperty);
            set => SetValue(ZoomPercentageProperty, value);
        }

        public bool RecenterZoombox
        {
            get => (bool)GetValue(RecenterZoomboxProperty);
            set => SetValue(RecenterZoomboxProperty, value);
        }

        public ICommand MiddleMouseClickedCommand
        {
            get => (ICommand)GetValue(MiddleMouseClickedCommandProperty);
            set => SetValue(MiddleMouseClickedCommandProperty, value);
        }

        public Point ViewportPosition
        {
            get => (Point)GetValue(ViewportPositionProperty);
            set => SetValue(ViewportPositionProperty, value);
        }

        public ICommand MouseMoveCommand
        {
            get => (ICommand)GetValue(MouseMoveCommandProperty);
            set => SetValue(MouseMoveCommandProperty, value);
        }

        public ICommand MouseDownCommand
        {
            get => (ICommand)GetValue(MouseDownCommandProperty);
            set => SetValue(MouseDownCommandProperty, value);
        }

        public double MouseXOnCanvas
        {
            get => (double)GetValue(MouseXOnCanvasProperty);
            set => SetValue(MouseXOnCanvasProperty, value);
        }

        public double MouseYOnCanvas
        {
            get => (double)GetValue(MouseYOnCanvasProperty);
            set => SetValue(MouseYOnCanvasProperty, value);
        }

        public bool GridLinesVisible
        {
            get => (bool)GetValue(GridLinesVisibleProperty);
            set => SetValue(GridLinesVisibleProperty, value);
        }
    }
}
