using PixiEditor.Models.DataHolders;
using PixiEditor.Models.ImageManipulation;
using PixiEditor.ViewModels;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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
            MouseEnter += PreviewWindow_MouseEnter;
            MouseLeave += PreviewWindow_MouseLeave;
        }

        private void PreviewWindow_MouseLeave(object sender, MouseEventArgs e)
        {
            if (ViewModelMain.Current != null)
            {
                ViewModelMain.Current.OverrideActionDisplay = false;
            }
        }

        private void PreviewWindow_MouseEnter(object sender, MouseEventArgs e)
        {
            if (ViewModelMain.Current != null)
            {
                ViewModelMain.Current.ActionDisplay = "Press right click to copy the color at the color cursor to the main color. Hold shift to copy the color the clipboard.";
                ViewModelMain.Current.OverrideActionDisplay = true;
            }
        }

        private void PreviewWindow_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                CopyColorToClipboard();
            }
            else
            {
                CopyColorToPrimary();
            }
        }

        private void CopyColorToPrimary()
        {
            PrimaryColor = ColorCursorColor;
        }

        private void CopyColorToClipboard()
        {
            if (ColorCursorColor.A == 255)
            {
                Clipboard.SetText(string.Format("#{0:X2}{1:X2}{2:X2}", ColorCursorColor.R, ColorCursorColor.G, ColorCursorColor.B));
            }
            else
            {
                Clipboard.SetText(ColorCursorColor.ToString());
            }
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
