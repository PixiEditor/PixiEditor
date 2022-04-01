using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.DataHolders.Palettes;
using PixiEditor.Models.DataProviders;
using PixiEditor.Models.IO;
using PixiEditor.Models.IO.JascPalFile;
using PixiEditor.Views.Dialogs;
using SkiaSharp;

namespace PixiEditor.Views.UserControls.Palettes
{
    /// <summary>
    /// Interaction logic for Palette.xaml
    /// </summary>
    public partial class PaletteViewer : UserControl
    {
        public static readonly DependencyProperty ColorsProperty = DependencyProperty.Register(
            "Colors", typeof(WpfObservableRangeCollection<SKColor>), typeof(PaletteViewer));

        public WpfObservableRangeCollection<SKColor> Colors
        {
            get { return (WpfObservableRangeCollection<SKColor>)GetValue(ColorsProperty); }
            set { SetValue(ColorsProperty, value); }
        }

        public Color HintColor
        {
            get { return (Color)GetValue(HintColorProperty); }
            set { SetValue(HintColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HintColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HintColorProperty =
            DependencyProperty.Register("HintColor", typeof(Color), typeof(PaletteViewer), new PropertyMetadata(System.Windows.Media.Colors.Transparent));

        public static readonly DependencyProperty ReplaceColorsCommandProperty = DependencyProperty.Register(
            "ReplaceColorsCommand", typeof(ICommand), typeof(PaletteViewer), new PropertyMetadata(default(ICommand)));

        public ICommand ReplaceColorsCommand
        {
            get { return (ICommand) GetValue(ReplaceColorsCommandProperty); }
            set { SetValue(ReplaceColorsCommandProperty, value); }
        }

        public ICommand SelectColorCommand
        {
            get { return (ICommand)GetValue(SelectColorCommandProperty); }
            set { SetValue(SelectColorCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectColorCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectColorCommandProperty =
            DependencyProperty.Register("SelectColorCommand", typeof(ICommand), typeof(PaletteViewer));

        public ICommand ImportPaletteCommand
        {
            get { return (ICommand)GetValue(ImportPaletteCommandProperty); }
            set { SetValue(ImportPaletteCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImportPaletteCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImportPaletteCommandProperty =
            DependencyProperty.Register("ImportPaletteCommand", typeof(ICommand), typeof(PaletteViewer));

        public WpfObservableRangeCollection<PaletteListDataSource> DataSources
        {
            get { return (WpfObservableRangeCollection<PaletteListDataSource>)GetValue(DataSourcesProperty); }
            set { SetValue(DataSourcesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DataSources.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataSourcesProperty =
            DependencyProperty.Register("DataSources", typeof(WpfObservableRangeCollection<PaletteListDataSource>), typeof(PaletteViewer), new PropertyMetadata(new WpfObservableRangeCollection<PaletteListDataSource>()));

        public WpfObservableRangeCollection<PaletteFileParser> FileParsers
        {
            get { return (WpfObservableRangeCollection<PaletteFileParser>)GetValue(FileParsersProperty); }
            set { SetValue(FileParsersProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FileParsers.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FileParsersProperty =
            DependencyProperty.Register("FileParsers", typeof(WpfObservableRangeCollection<PaletteFileParser>), typeof(PaletteViewer), new PropertyMetadata(new WpfObservableRangeCollection<PaletteFileParser>()));

        private string _filesFilter;

        public string FilesFilter
        {
            get
            {
                if(_filesFilter == null)
                {
                    string filter = "";

                    foreach (var parser in FileParsers)
                    {
                        string supportedFormats = string.Join(';', parser.SupportedFileExtensions).Replace(".", "*.");
                        filter += $"{parser.FileName} ({supportedFormats})|{supportedFormats}|";
                    }
                    
                    _filesFilter = filter.Remove(filter.Length - 1);
                }

                return _filesFilter;
            }
            set => _filesFilter = value;
        }


        private PaletteList _cachedPaletteList;

        public PaletteViewer()
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

        private async void ImportPalette_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = FilesFilter
            };
            if (openFileDialog.ShowDialog() == true)
            {
                await ImportPalette(openFileDialog.FileName);
            }
        }

        private async Task ImportPalette(string fileName)
        {
            var parser = FileParsers.FirstOrDefault(x => x.SupportedFileExtensions.Contains(Path.GetExtension(fileName)));
            var data = await parser.Parse(fileName);
            Colors.Clear();
            Colors.AddRange(data.Colors);
        }

        private async void SavePalette_OnClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = FilesFilter
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string fileName = saveFileDialog.FileName;
                var foundParser = FileParsers.First(x => x.SupportedFileExtensions.Contains(Path.GetExtension(fileName)));
                await foundParser.Save(fileName, new PaletteFileData(Colors.ToArray()));
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

        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            if(IsPalFilePresent(e, out string filePath))
            {
                await ImportPalette(filePath);
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

        private void PaletteColor_Drop(object sender, DragEventArgs e)
        {
            if(e.Data.GetDataPresent(PaletteColor.PaletteColorDaoFormat))
            {
                string data = (string)e.Data.GetData(PaletteColor.PaletteColorDaoFormat);
                SKColor color = SKColor.Parse(data);
                if(Colors.Contains(color))
                {
                    PaletteColor paletteColor = sender as PaletteColor;
                    int currIndex = Colors.IndexOf(color);
                    int newIndex = Colors.IndexOf(paletteColor.Color);
                    Colors.RemoveAt(currIndex);
                    Colors.Insert(newIndex, color);
                }
            }
        }

        private async void BrowsePalettes_Click(object sender, RoutedEventArgs e)
        {
            var browser = PalettesBrowser.Open(DataSources, ImportPaletteCommand, Colors);
            await browser.UpdatePaletteList();
        }

        private void ReplaceColor_OnClick(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            SKColor color = (SKColor)menuItem.CommandParameter;
            Replacer.ColorToReplace = color;
            Replacer.VisibilityCheckbox.IsChecked = false;
        }
    }
}
