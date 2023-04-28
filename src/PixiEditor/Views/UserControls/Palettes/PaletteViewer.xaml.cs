using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using PixiEditor.Helpers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.DataProviders;
using PixiEditor.Models.IO;
using PixiEditor.Views.Dialogs;
using BackendColor = PixiEditor.DrawingApi.Core.ColorsImpl.Color;

namespace PixiEditor.Views.UserControls.Palettes;

/// <summary>
/// Interaction logic for Palette.xaml
/// </summary>
internal partial class PaletteViewer : UserControl
{
    public static readonly DependencyProperty SwatchesProperty = DependencyProperty.Register(nameof(Swatches), typeof(WpfObservableRangeCollection<BackendColor>), typeof(PaletteViewer), new PropertyMetadata(default(WpfObservableRangeCollection<BackendColor>)));

    public WpfObservableRangeCollection<BackendColor> Swatches
    {
        get => (WpfObservableRangeCollection<BackendColor>)GetValue(SwatchesProperty);
        set => SetValue(SwatchesProperty, value);
    }
    public static readonly DependencyProperty ColorsProperty = DependencyProperty.Register(nameof(Colors), typeof(WpfObservableRangeCollection<BackendColor>), typeof(PaletteViewer));

    public WpfObservableRangeCollection<BackendColor> Colors
    {
        get { return (WpfObservableRangeCollection<BackendColor>)GetValue(ColorsProperty); }
        set { SetValue(ColorsProperty, value); }
    }

    public Color HintColor
    {
        get { return (Color)GetValue(HintColorProperty); }
        set { SetValue(HintColorProperty, value); }
    }


    public static readonly DependencyProperty HintColorProperty =
        DependencyProperty.Register(nameof(HintColor), typeof(Color), typeof(PaletteViewer), new PropertyMetadata(System.Windows.Media.Colors.Transparent));

    public static readonly DependencyProperty ReplaceColorsCommandProperty = DependencyProperty.Register(nameof(ReplaceColorsCommand), typeof(ICommand), typeof(PaletteViewer), new PropertyMetadata(default(ICommand)));

    public ICommand ReplaceColorsCommand
    {
        get { return (ICommand)GetValue(ReplaceColorsCommandProperty); }
        set { SetValue(ReplaceColorsCommandProperty, value); }
    }

    public ICommand SelectColorCommand
    {
        get { return (ICommand)GetValue(SelectColorCommandProperty); }
        set { SetValue(SelectColorCommandProperty, value); }
    }


    public static readonly DependencyProperty SelectColorCommandProperty =
        DependencyProperty.Register(nameof(SelectColorCommand), typeof(ICommand), typeof(PaletteViewer));

    public ICommand ImportPaletteCommand
    {
        get { return (ICommand)GetValue(ImportPaletteCommandProperty); }
        set { SetValue(ImportPaletteCommandProperty, value); }
    }


    public static readonly DependencyProperty ImportPaletteCommandProperty =
        DependencyProperty.Register(nameof(ImportPaletteCommand), typeof(ICommand), typeof(PaletteViewer));

    public WpfObservableRangeCollection<PaletteListDataSource> DataSources
    {
        get { return (WpfObservableRangeCollection<PaletteListDataSource>)GetValue(DataSourcesProperty); }
        set { SetValue(DataSourcesProperty, value); }
    }


    public static readonly DependencyProperty DataSourcesProperty =
        DependencyProperty.Register(nameof(DataSources), typeof(WpfObservableRangeCollection<PaletteListDataSource>), typeof(PaletteViewer), new PropertyMetadata(new WpfObservableRangeCollection<PaletteListDataSource>()));

    public WpfObservableRangeCollection<PaletteFileParser> FileParsers
    {
        get { return (WpfObservableRangeCollection<PaletteFileParser>)GetValue(FileParsersProperty); }
        set { SetValue(FileParsersProperty, value); }
    }


    public static readonly DependencyProperty FileParsersProperty =
        DependencyProperty.Register(nameof(FileParsers), typeof(WpfObservableRangeCollection<PaletteFileParser>), typeof(PaletteViewer), new PropertyMetadata(new WpfObservableRangeCollection<PaletteFileParser>()));

    public PaletteViewer()
    {
        InitializeComponent();
    }

    private void RemoveColorMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        MenuItem menuItem = (MenuItem)sender;
        BackendColor color = (BackendColor)menuItem.CommandParameter;
        if (Colors.Contains(color))
        {
            Colors.Remove(color);
        }
    }

    private async void ImportPalette_OnClick(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Filter = PaletteHelpers.GetFilter(FileParsers, true),
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
        if (data.IsCorrupted || data.Colors.Length == 0) return;
        Colors.Clear();
        Colors.AddRange(data.Colors);
    }

    private async void SavePalette_OnClick(object sender, RoutedEventArgs e)
    {
        SaveFileDialog saveFileDialog = new SaveFileDialog
        {
            Filter = PaletteHelpers.GetFilter(FileParsers, false)
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
        if (IsSupportedFilePresent(e, out _))
        {
            dragDropGrid.Visibility = Visibility.Visible;
            ViewModelMain.Current.ActionDisplays[nameof(PaletteViewer)] = "Import palette file";
        }
        else if (ColorHelper.ParseAnyFormatList(e.Data, out var list))
        {
            e.Effects = DragDropEffects.Copy;
            ViewModelMain.Current.ActionDisplays[nameof(PaletteViewer)] = list.Count > 1 ? "Import colors into palette" : "Import color into palette";
            e.Handled = true;
        }
    }

    private void Grid_PreviewDragLeave(object sender, DragEventArgs e)
    {
        dragDropGrid.Visibility = Visibility.Hidden;
        ViewModelMain.Current.ActionDisplays[nameof(PaletteViewer)] = null;
    }

    private async void Grid_Drop(object sender, DragEventArgs e)
    {
        ViewModelMain.Current.ActionDisplays[nameof(PaletteViewer)] = null;
        
        if (!IsSupportedFilePresent(e, out string filePath))
        {
            if (!ColorHelper.ParseAnyFormatList(e.Data, out var colors))
            {
                return;
            }
            
            e.Effects = DragDropEffects.Copy;
            Colors.AddRange(colors.Where(x => !Colors.Contains(x)).ToList());
            e.Handled = true;
            return;
        }

        await ImportPalette(filePath);
        dragDropGrid.Visibility = Visibility.Hidden;
    }

    private bool IsSupportedFilePresent(DragEventArgs e, out string filePath)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files is { Length: > 0 })
            {
                var fileName = files[0];
                var foundParser = FileParsers.FirstOrDefault(x => x.SupportedFileExtensions.Contains(Path.GetExtension(fileName)));
                if (foundParser != null)
                {
                    filePath = fileName;
                    return true;
                }
            }
        }

        filePath = null;
        return false;
    }

    private void PaletteColor_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(PaletteColor.PaletteColorDaoFormat))
        {
            string data = (string)e.Data.GetData(PaletteColor.PaletteColorDaoFormat);
            BackendColor color = BackendColor.Parse(data);
            if (Colors.Contains(color))
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
        BackendColor color = (BackendColor)menuItem.CommandParameter;
        Replacer.ColorToReplace = color;
        Replacer.VisibilityCheckbox.IsChecked = false;
    }

    private void MenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        MenuItem origin = (MenuItem)sender;
        if (SelectColorCommand.CanExecute(origin.CommandParameter))
        {
            SelectColorCommand.Execute(origin.CommandParameter);
        }
    }
}
