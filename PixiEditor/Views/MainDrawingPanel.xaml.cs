using PixiEditor.ViewModels;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using PixiEditor.Models.Tools.Tools;
using Xceed.Wpf.Toolkit.Core.Input;
using Xceed.Wpf.Toolkit.Zoombox;

namespace PixiEditor.Views
{
    /// <summary>
    ///     Interaction logic for MainDrawingPanel.xaml
    /// </summary>
    public partial class MainDrawingPanel : UserControl
    {
        // Using a DependencyProperty as the backing store for Center.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CenterProperty =
            DependencyProperty.Register("Center", typeof(bool), typeof(MainDrawingPanel),
                new PropertyMetadata(true, OnCenterChanged));

        // Using a DependencyProperty as the backing store for MouseX.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseXProperty =
            DependencyProperty.Register("MouseX", typeof(double), typeof(MainDrawingPanel), new PropertyMetadata(null));

        // Using a DependencyProperty as the backing store for MouseX.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseYProperty =
            DependencyProperty.Register("MouseY", typeof(double), typeof(MainDrawingPanel), new PropertyMetadata(null));

        // Using a DependencyProperty as the backing store for MouseMoveCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseMoveCommandProperty =
            DependencyProperty.Register("MouseMoveCommand", typeof(ICommand), typeof(MainDrawingPanel),
                new PropertyMetadata(null));

        // Using a DependencyProperty as the backing store for CenterOnStart.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CenterOnStartProperty =
            DependencyProperty.Register("CenterOnStart", typeof(bool), typeof(MainDrawingPanel),
                new PropertyMetadata(false));

        // Using a DependencyProperty as the backing store for Item.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("Item", typeof(object), typeof(MainDrawingPanel), new PropertyMetadata(default(FrameworkElement)));

        // Using a DependencyProperty as the backing store for Item.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsUsingZoomToolProperty =
            DependencyProperty.Register("IsUsingZoomTool", typeof(bool), typeof(MainDrawingPanel), new PropertyMetadata(false));


        public double ZoomPercentage
        {
            get { return (double)GetValue(ZoomPercentageProperty); }
            set { SetValue(ZoomPercentageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ZoomPercentage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ZoomPercentageProperty =
            DependencyProperty.Register("ZoomPercentage", typeof(double), typeof(MainDrawingPanel), new PropertyMetadata(0.0, ZoomPercentegeChanged));


        public bool Center
        {
            get => (bool) GetValue(CenterProperty);
            set => SetValue(CenterProperty, value);
        }

        public double MouseX
        {
            get => (double) GetValue(MouseXProperty);
            set => SetValue(MouseXProperty, value);
        }

        public double MouseY
        {
            get => (double) GetValue(MouseYProperty);
            set => SetValue(MouseYProperty, value);
        }


        public ICommand MouseMoveCommand
        {
            get => (ICommand) GetValue(MouseMoveCommandProperty);
            set => SetValue(MouseMoveCommandProperty, value);
        }


        public bool CenterOnStart
        {
            get => (bool) GetValue(CenterOnStartProperty);
            set => SetValue(CenterOnStartProperty, value);
        }


        public object Item
        {
            get => GetValue(ItemProperty);
            set => SetValue(ItemProperty, value);
        }

        public bool IsUsingZoomTool
        {
            get => (bool) GetValue(IsUsingZoomToolProperty);
            set => SetValue(IsUsingZoomToolProperty, value);
        }

        private static void ZoomPercentegeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MainDrawingPanel panel = (MainDrawingPanel)d;
            double percentage = (double)e.NewValue;
            if(percentage == 100)
            {
                panel.SetClickValues();
            }
            panel.Zoombox.ZoomTo(panel.ClickScale * ((double)e.NewValue / 100.0));
        }

        public double ClickScale;

        public MainDrawingPanel()
        {
            InitializeComponent();
            Zoombox.ZoomToSelectionModifiers = new KeyModifierCollection() { KeyModifier.RightAlt };
        }

        private void Zoombox_CurrentViewChanged(object sender, ZoomboxViewChangedEventArgs e)
        {
            Zoombox.MinScale = 32 / ((FrameworkElement)Item).Width;
            Zoombox.KeepContentInBounds = !(Zoombox.Scale > Zoombox.MinScale * 35.0);
        }

        private void Zoombox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ZoomPercentage == 100)
            {
                SetClickValues();
            }
        }

        private void SetClickValues()
        {
            if (!IsUsingZoomTool) return;
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

        private static void OnCenterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MainDrawingPanel panel = (MainDrawingPanel) d;
            panel.Zoombox.CenterContent();
        }


        private void Zoombox_Loaded(object sender, RoutedEventArgs e)
        {
            if (CenterOnStart) ((Zoombox) sender).CenterContent();
            ClickScale = Zoombox.Scale;
        }

        private void Zoombox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            SetZoomOrigin();
        }

        private void mainDrawingPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IsUsingZoomTool = ViewModelMain.Current.BitmapManager.SelectedTool is ZoomTool;
            Mouse.Capture((IInputElement)sender, CaptureMode.SubTree);
            SetClickValues();
        }

        private void mainDrawingPanel_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            ((IInputElement)sender).ReleaseMouseCapture();
        }
    }
}