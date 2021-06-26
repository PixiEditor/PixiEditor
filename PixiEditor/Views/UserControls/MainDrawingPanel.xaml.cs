using PixiEditor.Views.UserControls;
using System.Windows;
using System.Windows.Controls;

namespace PixiEditor.Views
{
    public partial class MainDrawingPanel : UserControl
    {
        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register(nameof(Item), typeof(object), typeof(MainDrawingPanel), new PropertyMetadata(default(FrameworkElement)));

        public static readonly DependencyProperty IsUsingZoomToolProperty =
            DependencyProperty.Register(nameof(IsUsingZoomTool), typeof(bool), typeof(MainDrawingPanel), new PropertyMetadata(false, ToolChanged));

        public static readonly DependencyProperty IsUsingMoveViewportToolProperty =
            DependencyProperty.Register(nameof(IsUsingMoveViewportTool), typeof(bool), typeof(MainDrawingPanel), new PropertyMetadata(false, ToolChanged));

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
        public bool IsUsingMoveViewportTool
        {
            get => (bool)GetValue(IsUsingMoveViewportToolProperty);
            set => SetValue(IsUsingMoveViewportToolProperty, value);
        }

        public MainDrawingPanel()
        {
            InitializeComponent();
        }

        private static void ToolChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var panel = (MainDrawingPanel)sender;
            if (panel.IsUsingZoomTool)
                panel.zoombox.ZoomMode = Zoombox.Mode.ZoomTool;
            else if (panel.IsUsingMoveViewportTool)
                panel.zoombox.ZoomMode = Zoombox.Mode.MoveTool;
            else
                panel.zoombox.ZoomMode = Zoombox.Mode.Normal;
        }
    }
}
