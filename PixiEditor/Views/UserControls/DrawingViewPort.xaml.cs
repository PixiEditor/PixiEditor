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

        public float ZoomPercentage
        {
            get { return (float)GetValue(ZoomPercentageProperty); }
            set { SetValue(ZoomPercentageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ZoomPercentage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ZoomPercentageProperty =
            DependencyProperty.Register("ZoomPercentage", typeof(float), typeof(DrawingViewPort), new PropertyMetadata(100f));

        public bool RecenterZoombox
        {
            get { return (bool)GetValue(RecenterZoomboxProperty); }
            set { SetValue(RecenterZoomboxProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RecenterZoombox.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RecenterZoomboxProperty =
            DependencyProperty.Register("RecenterZoombox", typeof(bool), typeof(DrawingViewPort), new PropertyMetadata(false));

        public ICommand MiddleMouseClickedCommand
        {
            get { return (ICommand)GetValue(MiddleMouseClickedCommandProperty); }
            set { SetValue(MiddleMouseClickedCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MiddleMouseClickedCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MiddleMouseClickedCommandProperty =
            DependencyProperty.Register("MiddleMouseClickedCommand", typeof(ICommand), typeof(DrawingViewPort), new PropertyMetadata(default(ICommand)));

        public Point ViewportPosition
        {
            get { return (Point)GetValue(ViewportPositionProperty); }
            set { SetValue(ViewportPositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewportPosition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewportPositionProperty =
            DependencyProperty.Register("ViewportPosition", typeof(Point), typeof(DrawingViewPort), new PropertyMetadata(default(Point)));

        public ICommand MouseMoveCommand
        {
            get { return (ICommand)GetValue(MouseMoveCommandProperty); }
            set { SetValue(MouseMoveCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MouseMoveCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseMoveCommandProperty =
            DependencyProperty.Register("MouseMoveCommand", typeof(ICommand), typeof(DrawingViewPort), new PropertyMetadata(default(ICommand)));

        public ICommand MouseDownCommand
        {
            get { return (ICommand)GetValue(MouseDownCommandProperty); }
            set { SetValue(MouseDownCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MouseDownCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseDownCommandProperty =
            DependencyProperty.Register("MouseDownCommand", typeof(ICommand), typeof(DrawingViewPort), new PropertyMetadata(default(ICommand)));

        public double MouseXOnCanvas
        {
            get { return (double)GetValue(MouseXOnCanvasProperty); }
            set { SetValue(MouseXOnCanvasProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MouseXOnCanvas.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseXOnCanvasProperty =
            DependencyProperty.Register("MouseXOnCanvas", typeof(double), typeof(DrawingViewPort), new PropertyMetadata(0.0));

        public double MouseYOnCanvas
        {
            get { return (double)GetValue(MouseYOnCanvasProperty); }
            set { SetValue(MouseYOnCanvasProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MouseXOnCanvas.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseYOnCanvasProperty =
            DependencyProperty.Register("MouseYOnCanvas", typeof(double), typeof(DrawingViewPort), new PropertyMetadata(0.0));

        public bool GridLinesVisible
        {
            get { return (bool)GetValue(GridLinesVisibleProperty); }
            set { SetValue(GridLinesVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GridLinesVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GridLinesVisibleProperty =
            DependencyProperty.Register("GridLinesVisible", typeof(bool), typeof(DrawingViewPort), new PropertyMetadata(false));
    }
}