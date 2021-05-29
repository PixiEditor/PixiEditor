using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PixiEditor.Views
{
    /// <summary>
    /// Interaction logic for Rotatebox.xaml
    /// </summary>
    public partial class Rotatebox : UserControl
    {
        private double _height = 0, _width = 0;
        private float _offset = 90;

        public Rotatebox()
        {
            InitializeComponent();
            MouseLeftButtonDown += OnMouseLeftButtonDown;
            MouseUp += OnMouseUp;
            MouseMove += OnMouseMove;
        }

        // Using a DependencyProperty backing store for Angle.
        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register("Angle", typeof(double), typeof(Rotatebox), new UIPropertyMetadata(0.0));

        public double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }


        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(this);
            _width = ActualWidth;
            _height = ActualHeight;
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (Equals(Mouse.Captured, this))
            {
                // Get the current mouse position relative to the control
                Point currentLocation = Mouse.GetPosition(this);

                // We want to rotate around the center of the knob, not the top corner
                Point knobCenter = new Point(_width / 2, _height / 2);

                // Calculate an angle
                double radians = Math.Atan((currentLocation.Y - knobCenter.Y) /
                                           (currentLocation.X - knobCenter.X));
                Angle = radians * 180 / Math.PI + _offset;

                // Apply a 180 degree shift when X is negative so that we can rotate
                // all of the way around
                if (currentLocation.X - knobCenter.X < 0)
                {
                    Angle += 180;
                }
            }
        }
    }
}
