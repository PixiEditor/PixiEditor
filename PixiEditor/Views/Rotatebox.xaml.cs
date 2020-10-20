using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PixiEditor.Views
{
    /// <summary>
    ///     Interaction logic for Rotatebox.xaml
    /// </summary>
    public partial class Rotatebox : UserControl
    {
        // Using a DependencyProperty backing store for Angle.
        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register("Angle", typeof(double), typeof(Rotatebox), new UIPropertyMetadata(0.0));

        private double height, width;
        private readonly float offset = 90;

        public Rotatebox()
        {
            InitializeComponent();
            MouseLeftButtonDown += OnMouseLeftButtonDown;
            MouseUp += OnMouseUp;
            MouseMove += OnMouseMove;
        }

        public double Angle
        {
            get => (double) GetValue(AngleProperty);
            set => SetValue(AngleProperty, value);
        }


        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(this);
            width = ActualWidth;
            height = ActualHeight;
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
                var currentLocation = Mouse.GetPosition(this);

                // We want to rotate around the center of the knob, not the top corner
                var knobCenter = new Point(width / 2, height / 2);

                // Calculate an angle
                var radians = Math.Atan((currentLocation.Y - knobCenter.Y) /
                                        (currentLocation.X - knobCenter.X));
                Angle = radians * 180 / Math.PI + offset;

                // Apply a 180 degree shift when X is negative so that we can rotate
                // all of the way around
                if (currentLocation.X - knobCenter.X < 0) Angle += 180;
            }
        }
    }
}