using PixiEditor.Models.DataHolders;
using PixiEditor.Models.ImageManipulation;
using PixiEditor.Models.Layers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PixiEditor.Views.UserControls
{
    /// <summary>
    /// Interaction logic for PreviewWindow.xaml
    /// </summary>
    public partial class PreviewWindow : UserControl
    {
        public static readonly DependencyProperty DocumentProperty =
            DependencyProperty.Register(nameof(Document), typeof(Document), typeof(PreviewWindow));

        public Document Document
        { 
            get => (Document)GetValue(DocumentProperty);
            set => SetValue(DocumentProperty, value);
        }

        public static readonly DependencyProperty ColorCursorPositionProperty =
            DependencyProperty.Register(nameof(ColorCursorPosition), typeof(Thickness), typeof(PreviewWindow));

        public Thickness ColorCursorPosition
        {
            get => (Thickness)GetValue(ColorCursorPositionProperty);
            private set => SetValue(ColorCursorPositionProperty, value);
        }

        public static readonly DependencyProperty ColorCursorColorProperty =
            DependencyProperty.Register(nameof(ColorCursorColor), typeof(Color), typeof(PreviewWindow));

        public Color ColorCursorColor
        {
            get => (Color)GetValue(ColorCursorColorProperty);
            set => SetValue(ColorCursorColorProperty, value);
        }

        public static readonly DependencyProperty PrimaryColorProperty =
            DependencyProperty.Register(nameof(PrimaryColor), typeof(Color), typeof(PreviewWindow));

        public Color PrimaryColor
        {
            get => (Color)GetValue(PrimaryColorProperty);
            set => SetValue(PrimaryColorProperty, value);
        }

        public PreviewWindow()
        {
            InitializeComponent();

            MouseMove += PreviewWindow_MouseMove;
            MouseRightButtonDown += PreviewWindow_MouseRightButtonDown;
        }

        private void PreviewWindow_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            PrimaryColor = ColorCursorColor;
        }

        private void PreviewWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (Document == null)
            {
                return;
            }

            Point mousePos = e.GetPosition(imageGrid);

            int x = (int)mousePos.X;
            int y = (int)mousePos.Y;

            Thickness newPos = new Thickness(x, y, 0, 0);

            if (ColorCursorPosition == newPos)
            {
                return;
            }

            ColorCursorPosition = newPos;

            ColorCursorColor = BitmapUtils.GetColorAtPointCombined(x, y, Document.Layers.ToArray());
        }
    }
}
