using PixiEditor.Models.Tools.Tools;
using PixiEditor.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xceed.Wpf.Toolkit.Core.Input;
using Xceed.Wpf.Toolkit.Zoombox;

namespace PixiEditor.Views
{
    public partial class MainDrawingPanel : UserControl
    {
        public static readonly DependencyProperty CenterProperty =
            DependencyProperty.Register(nameof(Center), typeof(bool), typeof(MainDrawingPanel),
                new PropertyMetadata(true, OnCenterChanged));

        public static readonly DependencyProperty MouseXProperty =
            DependencyProperty.Register(nameof(MouseX), typeof(double), typeof(MainDrawingPanel), new PropertyMetadata(null));

        public static readonly DependencyProperty MouseYProperty =
            DependencyProperty.Register(nameof(MouseY), typeof(double), typeof(MainDrawingPanel), new PropertyMetadata(null));

        public static readonly DependencyProperty MouseMoveCommandProperty =
            DependencyProperty.Register(nameof(MouseMoveCommand), typeof(ICommand), typeof(MainDrawingPanel),
                new PropertyMetadata(null));

        public static readonly DependencyProperty CenterOnStartProperty =
            DependencyProperty.Register(nameof(CenterOnStart), typeof(bool), typeof(MainDrawingPanel),
                new PropertyMetadata(false));

        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register(nameof(Item), typeof(object), typeof(MainDrawingPanel), new PropertyMetadata(default(FrameworkElement)));

        public static readonly DependencyProperty IsUsingZoomToolProperty =
            DependencyProperty.Register(nameof(IsUsingZoomTool), typeof(bool), typeof(MainDrawingPanel), new PropertyMetadata(false));

        public static readonly DependencyProperty ZoomPercentageProperty =
            DependencyProperty.Register(nameof(ZoomPercentage), typeof(double), typeof(MainDrawingPanel),
                new PropertyMetadata(0.0, ZoomPercentegeChanged));

        public static readonly DependencyProperty ViewportPositionProperty =
            DependencyProperty.Register(nameof(ViewportPosition), typeof(Point), typeof(MainDrawingPanel),
                new PropertyMetadata(default(Point), ViewportPosCallback));

        public static readonly DependencyProperty MiddleMouseClickedCommandProperty =
            DependencyProperty.Register(nameof(MiddleMouseClickedCommand), typeof(ICommand), typeof(MainDrawingPanel), new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty MiddleMouseClickedCommandParameterProperty =
            DependencyProperty.Register(nameof(MiddleMouseClickedCommandParameter), typeof(object), typeof(MainDrawingPanel), new PropertyMetadata(default(object)));

        public bool Center
        {
            get => (bool)GetValue(CenterProperty);
            set => SetValue(CenterProperty, value);
        }

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

        public bool CenterOnStart
        {
            get => (bool)GetValue(CenterOnStartProperty);
            set => SetValue(CenterOnStartProperty, value);
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

        public double ZoomPercentage
        {
            get => (double)GetValue(ZoomPercentageProperty);
            set => SetValue(ZoomPercentageProperty, value);
        }

        public Point ViewportPosition
        {
            get => (Point)GetValue(ViewportPositionProperty);
            set => SetValue(ViewportPositionProperty, value);
        }

        public ICommand MiddleMouseClickedCommand
        {
            get => (ICommand)GetValue(MiddleMouseClickedCommandProperty);
            set => SetValue(MiddleMouseClickedCommandProperty, value);
        }

        public object MiddleMouseClickedCommandParameter
        {
            get => GetValue(MiddleMouseClickedCommandParameterProperty);
            set => SetValue(MiddleMouseClickedCommandParameterProperty, value);
        }

        public double ClickScale;
        public Point ClickPosition;

        public MainDrawingPanel()
        {
            InitializeComponent();
            Zoombox.ZoomToSelectionModifiers = new KeyModifierCollection() { KeyModifier.RightAlt };
        }

        private static void ZoomPercentegeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MainDrawingPanel panel = (MainDrawingPanel)d;
            double percentage = (double)e.NewValue;
            if (percentage == 100)
            {
                panel.SetClickValues();
            }
            panel.Zoombox.ZoomTo(panel.ClickScale * ((double)e.NewValue / 100.0));
        }

        private static void ViewportPosCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MainDrawingPanel panel = (MainDrawingPanel)d;
            if (PresentationSource.FromVisual(panel.Zoombox) == null)
            {
                panel.Zoombox.Position = default;
                return;
            }
            TranslateZoombox(panel, (Point)e.NewValue);
        }

        private static void TranslateZoombox(MainDrawingPanel panel, Point vector)
        {
            var newPos = new Point(
                panel.ClickPosition.X + vector.X,
                panel.ClickPosition.Y + vector.Y);
            panel.Zoombox.Position = newPos;
        }

        private static void OnCenterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MainDrawingPanel panel = (MainDrawingPanel)d;
            panel.Zoombox.FitToBounds();
        }

        private void SetClickValues()
        {
            if (!IsUsingZoomTool)
            {
                return;
            }

            ClickScale = Zoombox.Scale;
            SetZoomOrigin();
        }

        private void SetZoomOrigin()
        {
            var item = (FrameworkElement)Item;
            if (item == null) return;
            var mousePos = Mouse.GetPosition(item);
            Zoombox.ZoomOrigin = new Point(Math.Clamp(mousePos.X / item.Width, 0, 1), Math.Clamp(mousePos.Y / item.Height, 0, 1));
        }
        private void MainDrawingPanel_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            IsUsingZoomTool = ViewModelMain.Current.BitmapManager.SelectedTool is ZoomTool;
            Mouse.Capture((IInputElement)sender, CaptureMode.SubTree);
            ClickPosition = ((FrameworkElement)Item).TranslatePoint(new Point(0, 0), Zoombox);
            SetClickValues();
        }

        private void MainDrawingPanel_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            ((IInputElement)sender).ReleaseMouseCapture();
        }

        private void Zoombox_Loaded(object sender, RoutedEventArgs e)
        {
            if (CenterOnStart)
            {
                ((Zoombox)sender).FitToBounds();
            }

            ClickScale = Zoombox.Scale;
        }

        private void Zoombox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            SetZoomOrigin();
        }

        private void Zoombox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed &&
                MiddleMouseClickedCommand.CanExecute(MiddleMouseClickedCommandParameter))
            {
                MiddleMouseClickedCommand.Execute(MiddleMouseClickedCommandParameter);
            }
        }
        private void Zoombox_CurrentViewChanged(object sender, ZoomboxViewChangedEventArgs e)
        {
            Zoombox.MinScale = 32 / ((FrameworkElement)Item).Width;
        }

        private void Zoombox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ZoomPercentage == 100)
            {
                SetClickValues();
            }
        }
    }
}
