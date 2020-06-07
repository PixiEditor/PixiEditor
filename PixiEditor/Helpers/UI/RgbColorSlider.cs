using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixiEditor.Helpers.UI
{
    public class RgbColorSlider : Slider
    {
        public RgbColorSlider()
        {
            Minimum = 0;
            Maximum = 255;
            SmallChange = 1;
            LargeChange = 10;
            MinHeight = 12;
        }

        public override void EndInit()
        {
            base.EndInit();
            GenerateBackground();
        }


        private void GenerateBackground()
        {
            Background = new LinearGradientBrush(new GradientStopCollection() {
                new GradientStop(GetColorForSelectedArgb(0), 0.0),
                new GradientStop(GetColorForSelectedArgb(255), 1),
            });
        }

        private Color GetColorForSelectedArgb(byte value)
        {
            return SliderArgbType switch
            {
                "A" => Color.FromArgb(value, CurrentColor.R, CurrentColor.G, CurrentColor.B),
                "R" => Color.FromArgb(CurrentColor.A, value, CurrentColor.G, CurrentColor.B),
                "G" => Color.FromArgb(CurrentColor.A, CurrentColor.R, value, CurrentColor.B),
                "B" => Color.FromArgb(CurrentColor.A, CurrentColor.R, CurrentColor.G, value),
                _ => CurrentColor,
            };
        }



        public string SliderArgbType
        {
            get { return (string)GetValue(SliderArgbTypeProperty); }
            set { SetValue(SliderArgbTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SliderArgbType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SliderArgbTypeProperty =
            DependencyProperty.Register("SliderArgbType", typeof(string), typeof(RgbColorSlider), new PropertyMetadata(""));



        public Color CurrentColor
        {
            get { return (Color)GetValue(CurrentColorProperty); }
            set { SetValue(CurrentColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentColorProperty =
            DependencyProperty.Register("CurrentColor", typeof(Color), typeof(RgbColorSlider), 
                new PropertyMetadata(Colors.Black, ColorChangedCallback));

        private static void ColorChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RgbColorSlider slider = (RgbColorSlider)d;
            slider.GenerateBackground();
        }
    }
}
