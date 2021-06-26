using PixiEditor.Helpers;
using PixiEditor.Models.Tools.Tools;
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
        public static readonly DependencyProperty MiddleMouseClickedCommandProperty =
            DependencyProperty.Register(nameof(MiddleMouseClickedCommand), typeof(ICommand), typeof(DrawingViewPort), new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty MouseMoveCommandProperty =
            DependencyProperty.Register(nameof(MouseMoveCommand), typeof(ICommand), typeof(DrawingViewPort), new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty MouseDownCommandProperty =
            DependencyProperty.Register(nameof(MouseDownCommand), typeof(ICommand), typeof(DrawingViewPort), new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty MouseXOnCanvasProperty =
            DependencyProperty.Register(nameof(MouseXOnCanvas), typeof(double), typeof(DrawingViewPort), new PropertyMetadata(0.0));

        public static readonly DependencyProperty MouseYOnCanvasProperty =
            DependencyProperty.Register(nameof(MouseYOnCanvas), typeof(double), typeof(DrawingViewPort), new PropertyMetadata(0.0));

        public static readonly DependencyProperty GridLinesVisibleProperty =
            DependencyProperty.Register(nameof(GridLinesVisible), typeof(bool), typeof(DrawingViewPort), new PropertyMetadata(false));

        public static readonly DependencyProperty IsUsingZoomToolProperty =
            DependencyProperty.Register(nameof(IsUsingZoomTool), typeof(bool), typeof(DrawingViewPort), new PropertyMetadata(false));

        public static readonly DependencyProperty IsUsingMoveViewportToolProperty =
            DependencyProperty.Register(nameof(IsUsingMoveViewportTool), typeof(bool), typeof(DrawingViewPort), new PropertyMetadata(false));

        public ICommand MiddleMouseClickedCommand
        {
            get => (ICommand)GetValue(MiddleMouseClickedCommandProperty);
            set => SetValue(MiddleMouseClickedCommandProperty, value);
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
        public bool IsUsingZoomTool
        {
            get => (bool)GetValue(IsUsingZoomToolProperty);
            set => SetValue(IsUsingZoomToolProperty, value);
        }
        public bool IsUsingMoveViewportTool
        {
            get => (bool)GetValue(IsUsingMoveViewportToolProperty);
            set => SetValue(IsUsingMoveViewportToolProperty, value);
        }

        public RelayCommand PreviewMouseDownCommand { get; private set; }

        public DrawingViewPort()
        {
            PreviewMouseDownCommand = new RelayCommand(ProcessMouseDown);
            InitializeComponent();
        }

        private void ProcessMouseDown(object parameter)
        {
            if (Mouse.MiddleButton == MouseButtonState.Pressed && MiddleMouseClickedCommand.CanExecute(typeof(MoveViewportTool)))
                MiddleMouseClickedCommand.Execute(typeof(MoveViewportTool));
        }
    }
}
