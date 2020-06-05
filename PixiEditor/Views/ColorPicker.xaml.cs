using PixiEditor.Models.Tools.Tools;
using System;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PixiEditor.Views
{
    /// <summary>
    /// Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : UserControl
    {
        private Image _colorPalette;

        public ColorPicker()
        {
            InitializeComponent();
            _colorPalette = (FindName("colorPalette") as Image);
        }

        public Color SelectedColor
        {
            get { return (Color)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register("SelectedColor", typeof(Color), typeof(ColorPicker), new PropertyMetadata(Colors.White));



        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point pos = e.GetPosition(_colorPalette);
                pos.X = Math.Clamp(pos.X, 0, _colorPalette.ActualWidth);
                pos.Y = Math.Abs(Math.Clamp(pos.Y, 0, _colorPalette.ActualHeight) - _colorPalette.ActualHeight);
                int h = (int)(pos.X * 360f / _colorPalette.ActualWidth);
                float l = (float)(pos.Y * 100f / _colorPalette.ActualHeight);
                SelectedColor = Models.Colors.ExColor.HslToRGB(h, 100, l);
            }
        }
    }
}
