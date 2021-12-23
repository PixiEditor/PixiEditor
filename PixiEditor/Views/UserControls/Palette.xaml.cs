using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.IO.JascPalFile;
using SkiaSharp;

namespace PixiEditor.Views.UserControls
{
    /// <summary>
    /// Interaction logic for Palette.xaml
    /// </summary>
    public partial class Palette : UserControl
    {
        public static readonly DependencyProperty ColorsProperty = DependencyProperty.Register(
            "Colors", typeof(ObservableCollection<SKColor>), typeof(Palette));

        public ObservableCollection<SKColor> Colors
        {
            get { return (ObservableCollection<SKColor>)GetValue(ColorsProperty); }
            set { SetValue(ColorsProperty, value); }
        }

        public ICommand SelectColorCommand
        {
            get { return (ICommand)GetValue(SelectColorCommandProperty); }
            set { SetValue(SelectColorCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectColorCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectColorCommandProperty =
            DependencyProperty.Register("SelectColorCommand", typeof(ICommand), typeof(Palette));

        public Palette()
        {
            InitializeComponent();
        }

        private void RemoveColorMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            SKColor color = (SKColor)menuItem.CommandParameter;
            if (Colors.Contains(color))
            {
                Colors.Remove(color);
            }
        }

        private void ImportPalette_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Palette (*.pal)|*.pal"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                ImportPallete(openFileDialog.FileName);
            }
        }

        private void ImportPallete(string fileName)
        {
            var jascData = JascFileParser.Parse(fileName);
            Colors.Clear();
            Colors.AddRange(jascData.Colors);
        }

        private void SavePalette_OnClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Palette (*.pal)|*.pal"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                string fileName = saveFileDialog.FileName;
                JascFileParser.Save(fileName, new JascFileData(Colors.ToArray()));
            }
        }

        private void Grid_PreviewDragEnter(object sender, DragEventArgs e)
        {
            if(IsPalFilePresent(e, out _))
            {
                dragDropGrid.Visibility = Visibility.Visible;
            }
        }

        private void Grid_PreviewDragLeave(object sender, DragEventArgs e)
        {
            dragDropGrid.Visibility = Visibility.Hidden;
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            if(IsPalFilePresent(e, out string filePath))
            {
                ImportPallete(filePath);
                dragDropGrid.Visibility = Visibility.Hidden;
            }
        }

        private bool IsPalFilePresent(DragEventArgs e, out string filePath)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0 && files[0].EndsWith(".pal"))
                {
                    filePath = files[0];
                    return true;
                }
            }

            filePath = null;
            return false;
        }

        private void PaletteColor_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }
    }
}
