using PixiEditor.ViewModels;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
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



        public double ZoomPercentage
        {
            get { return (double)GetValue(ZoomPrecentageProperty); }
            set { SetValue(ZoomPrecentageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ZoomPrecentage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ZoomPrecentageProperty =
            DependencyProperty.Register("ZoomPercentage", typeof(double), typeof(MainDrawingPanel), new PropertyMetadata(0.0, ZoomPercentegeChanged));

        private static void ZoomPercentegeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MainDrawingPanel panel = (MainDrawingPanel)d;
            double percentage = (double)e.NewValue;
            if(percentage == 100)
            {
                panel.ClickScale = panel.Zoombox.Scale;
            }
            panel.Zoombox.ZoomTo(panel.ClickScale * ((double)e.NewValue / 100.0));
        }

        public double ClickScale;
        public Point ClickPoint;

        public MainDrawingPanel()
        {
            InitializeComponent();
            Zoombox.ZoomToSelectionModifiers = new KeyModifierCollection() { KeyModifier.RightAlt };
        }

        private void Zoombox_CurrentViewChanged(object sender, ZoomboxViewChangedEventArgs e)
        {
            Zoombox.MinScale = 32 / ((FrameworkElement)Item).Width;
            if(Zoombox.Scale > Zoombox.MinScale * 35)
            {
                Zoombox.KeepContentInBounds = false;
            }
            else
            {
                Zoombox.KeepContentInBounds = true;
            }
        }

        private void Zoombox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SetClickValues();
        }

        private void SetClickValues()
        {
            ClickScale = Zoombox.Scale;
            var item = (FrameworkElement)Item;
            var mousePos = Mouse.GetPosition(item);
            Zoombox.ZoomOrigin = new Point(mousePos.X / item.Width, mousePos.Y / item.Height);
        }

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
            Point point = new Point(0.5, 0.5);
            if (Zoombox.ZoomOrigin != point)
            {
                Zoombox.CenterContent();
                Zoombox.ZoomOrigin = point;
            }
        }
    }
}