using GalaSoft.MvvmLight.Command;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PixiEditor.Views
{
    /// <summary>
    /// Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : UserControl, INotifyPropertyChanged
    {
        private Image _colorPalette;

        public ColorPicker()
        {
            InitializeComponent();
            _colorPalette = (FindName("colorPalette") as Image);
            _dispatcher = Application.Current.Dispatcher;
            NotifyableColor = new NotifyableColor(SelectedColor);
            NotifyableColor.ColorChanged += SelectedColor_ColorChanged;
        }

        private void SelectedColor_ColorChanged(object sender, EventArgs e)
        {
            SelectedColor = Color.FromArgb(NotifyableColor.A, NotifyableColor.R, NotifyableColor.G, NotifyableColor.B);            
        }

        private void SwapColors()
        {
            Color tmp = SecondaryColor;
            SecondaryColor = SelectedColor;
            SelectedColor = tmp;
        }

        private NotifyableColor _notifyableColor;

        public NotifyableColor NotifyableColor
        {
            get { return _notifyableColor; }
            set 
            { 
                _notifyableColor = value;
                RaisePropertyChanged("NotifyableColor");
            }
        }


        public Color SelectedColor
        {
            get { return (Color)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register("SelectedColor", typeof(Color), typeof(ColorPicker), 
                new PropertyMetadata(Colors.Black, OnSelectedColorChanged));

        private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Color color = (Color)e.NewValue;
            ((ColorPicker)d).NotifyableColor.SetArgb(color.A, color.R, color.G, color.B);
        }

        public Color SecondaryColor
        {
            get { return (Color)GetValue(SecondaryColorProperty); }
            set { SetValue(SecondaryColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SecondaryColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SecondaryColorProperty =
            DependencyProperty.Register("SecondaryColor", typeof(Color), typeof(ColorPicker), new PropertyMetadata(Colors.White));

        
        private Dispatcher _dispatcher;
        private System.Timers.Timer _timer = new System.Timers.Timer(5);

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void CalculateColor(Point pos)
        {
            pos.X = Math.Clamp(pos.X, 0, _colorPalette.ActualWidth);
            pos.Y = Math.Abs(Math.Clamp(pos.Y, 0, _colorPalette.ActualHeight) - _colorPalette.ActualHeight);
            int h = (int)(pos.X * 360f / _colorPalette.ActualWidth);
            float l = (float)(pos.Y * 100f / _colorPalette.ActualHeight);

            SelectedColor = Models.Colors.ExColor.HslToRGB(h, 100, l);
        }

        private void RaisePropertyChanged(string property)
        {
            if (property != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SwapColors();            
        }

        private void colorPalette_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _dispatcher.Invoke(() =>
            {
                System.Drawing.Point point = MousePositionConverter.GetCursorPosition();
                Point relativePoint = _colorPalette.PointFromScreen(new Point(point.X, point.Y));
                CalculateColor(relativePoint);
            });
        }

        private void colorPalette_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _timer.Stop();
        }
    }
}
