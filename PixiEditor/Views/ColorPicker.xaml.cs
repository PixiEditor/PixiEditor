using System;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using PixiEditor.Models.Colors;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Position;

namespace PixiEditor.Views
{
    /// <summary>
    ///     Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : UserControl, INotifyPropertyChanged
    {
        // Using a DependencyProperty as the backing store for SelectedColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register("SelectedColor", typeof(Color), typeof(ColorPicker),
                new PropertyMetadata(Colors.Black, OnSelectedColorChanged));

        // Using a DependencyProperty as the backing store for SecondaryColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SecondaryColorProperty =
            DependencyProperty.Register("SecondaryColor", typeof(Color), typeof(ColorPicker),
                new PropertyMetadata(Colors.White));

        private readonly Image _colorPalette;


        private readonly Dispatcher _dispatcher;

        private NotifyableColor _notifyableColor;
        private readonly Timer _timer = new Timer(5);

        public ColorPicker()
        {
            InitializeComponent();
            _colorPalette = FindName("colorPalette") as Image;
            _dispatcher = Application.Current.Dispatcher;
            NotifyableColor = new NotifyableColor(SelectedColor);
            NotifyableColor.ColorChanged += SelectedColor_ColorChanged;
            _colorPalette.IsVisibleChanged += _colorPalette_IsVisibleChanged;
        }

        public NotifyableColor NotifyableColor
        {
            get => _notifyableColor;
            set
            {
                _notifyableColor = value;
                RaisePropertyChanged("NotifyableColor");
            }
        }


        public Color SelectedColor
        {
            get => (Color) GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }

        public Color SecondaryColor
        {
            get => (Color) GetValue(SecondaryColorProperty);
            set => SetValue(SecondaryColorProperty, value);
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void _colorPalette_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _timer.Stop();
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

        private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Color color = (Color) e.NewValue;
            ((ColorPicker) d).NotifyableColor.SetArgb(color.A, color.R, color.G, color.B);
        }

        private void CalculateColor(Point pos)
        {
            pos.X = Math.Clamp(pos.X, 0, _colorPalette.ActualWidth);
            pos.Y = Math.Abs(Math.Clamp(pos.Y, 0, _colorPalette.ActualHeight) - _colorPalette.ActualHeight);
            int h = (int) (pos.X * 360f / _colorPalette.ActualWidth);
            float l = (float) (pos.Y * 100f / _colorPalette.ActualHeight);

            SelectedColor = ExColor.HslToRGB(h, 100, l);
        }

        private void RaisePropertyChanged(string property)
        {
            if (property != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
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

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _dispatcher.Invoke(() =>
            {
                if (Mouse.LeftButton == MouseButtonState.Released)
                {
                    _timer.Stop();
                    return;
                }

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