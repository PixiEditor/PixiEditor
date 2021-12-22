using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
                string fileName = openFileDialog.FileName;
                var jascData = JascFileParser.Parse(fileName);
                Colors.Clear();
                Colors.AddRange(jascData.Colors);
            }
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
    }
}
