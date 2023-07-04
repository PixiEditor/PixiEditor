using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using PixiEditor.Extensions.Palettes;
using PixiEditor.Extensions.Palettes.Parsers;
using PixiEditor.Helpers;
using PixiEditor.Models.AppExtensions.Services;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.DataProviders;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Enums;
using PixiEditor.Models.IO;
using PixiEditor.Views.Dialogs;

namespace PixiEditor.Views.UserControls.Palettes;

/// <summary>
/// Interaction logic for Palette.xaml
/// </summary>
internal partial class PaletteViewer : UserControl
{
    public static readonly DependencyProperty SwatchesProperty = DependencyProperty.Register(nameof(Swatches), typeof(WpfObservableRangeCollection<PaletteColor>), typeof(PaletteViewer), new PropertyMetadata(default(WpfObservableRangeCollection<PaletteColorControl>)));

    public WpfObservableRangeCollection<PaletteColor> Swatches
    {
        get => (WpfObservableRangeCollection<PaletteColor>)GetValue(SwatchesProperty);
        set => SetValue(SwatchesProperty, value);
    }
    public static readonly DependencyProperty ColorsProperty = DependencyProperty.Register(nameof(Colors), typeof(WpfObservableRangeCollection<PaletteColor>), typeof(PaletteViewer));

    public WpfObservableRangeCollection<PaletteColor> Colors
    {
        get { return (WpfObservableRangeCollection<PaletteColor>)GetValue(ColorsProperty); }
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

    public static readonly DependencyProperty PaletteProviderProperty = DependencyProperty.Register(
        nameof(PaletteProvider), typeof(PaletteProvider), typeof(PaletteViewer), new PropertyMetadata(default(PaletteProvider)));

    public PaletteProvider PaletteProvider
    {
        get { return (PaletteProvider)GetValue(PaletteProviderProperty); }
        set { SetValue(PaletteProviderProperty, value); }
    }
    
    public PaletteViewer()
    {
        InitializeComponent();
    }

    private void RemoveColorMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        MenuItem menuItem = (MenuItem)sender;
        PaletteColor colorControl = (PaletteColor)menuItem.CommandParameter;
        if (Colors.Contains(colorControl))
        {
            Colors.Remove(colorControl);
        }
    }

    private async void ImportPalette_OnClick(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Filter = PaletteHelpers.GetFilter(PaletteProvider.AvailableParsers, true),
        };
        if (openFileDialog.ShowDialog() == true)
        {
            await ImportPalette(openFileDialog.FileName);
        }
    }

    private async Task ImportPalette(string fileName)
    {
        var parser = PaletteProvider.AvailableParsers.FirstOrDefault(x => x.SupportedFileExtensions.Contains(Path.GetExtension(fileName)));
        if (parser == null) return;
        var data = await parser.Parse(fileName);
        if (data.IsCorrupted || data.Colors.Length == 0) return;
        Colors.Clear();
        Colors.AddRange(data.Colors);
    }

    private async void SavePalette_OnClick(object sender, RoutedEventArgs e)
    {
        SaveFileDialog saveFileDialog = new SaveFileDialog
        {
            Filter = PaletteHelpers.GetFilter(PaletteProvider.AvailableParsers.Where(x => x.CanSave).ToList(), false)
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            string fileName = saveFileDialog.FileName;
            var foundParser = PaletteProvider.AvailableParsers.First(x => x.SupportedFileExtensions.Contains(Path.GetExtension(fileName)));
            if (Colors == null || Colors.Count == 0)
            {
                NoticeDialog.Show("NO_COLORS_TO_SAVE", "ERROR");
                return;
            }
            bool saved = await foundParser.Save(fileName, new PaletteFileData(Colors.ToArray()));
            if (!saved)
            {
                NoticeDialog.Show("COULD_NOT_SAVE_PALETTE", "ERROR");
            }
        }
    }

    private void Grid_PreviewDragEnter(object sender, DragEventArgs e)
    {
        if (IsSupportedFilePresent(e, out _))
        {
            dragDropGrid.Visibility = Visibility.Visible;
            ViewModelMain.Current.ActionDisplays[nameof(PaletteViewer)] = "IMPORT_PALETTE_FILE";
        }
        else if (ColorHelper.ParseAnyFormatList(e.Data, out var list))
        {
            e.Effects = DragDropEffects.Copy;
            ViewModelMain.Current.ActionDisplays[nameof(PaletteViewer)] = list.Count > 1 ? "IMPORT_MULTIPLE_PALETTE_COLORS" : "IMPORT_SINGLE_PALETTE_COLOR";
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

            List<PaletteColor> paletteColors = colors.Select(x => new PaletteColor(x.R, x.G, x.B)).ToList();
            
            e.Effects = DragDropEffects.Copy;
            Colors.AddRange(paletteColors.Where(x => !Colors.Contains(new PaletteColor(x.R, x.G, x.B))).ToList());
            e.Handled = true;
            return;
        }

        e.Handled = true;
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
                var foundParser = PaletteProvider.AvailableParsers.FirstOrDefault(x => x.SupportedFileExtensions.Contains(Path.GetExtension(fileName)));
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
        if (e.Data.GetDataPresent(PaletteColorControl.PaletteColorDaoFormat))
        {
            string data = (string)e.Data.GetData(PaletteColorControl.PaletteColorDaoFormat);

            PaletteColor paletteColor = PaletteColor.Parse(data);
            if (Colors.Contains(paletteColor))
            {
                PaletteColorControl paletteColorControl = sender as PaletteColorControl;
                int currIndex = Colors.IndexOf(paletteColor);
                if (paletteColorControl != null)
                {
                    int newIndex = Colors.IndexOf(paletteColorControl.Color);
                    Colors.RemoveAt(currIndex);
                    Colors.Insert(newIndex, paletteColor);
                }
            }
        }
    }

    private async void BrowsePalettes_Click(object sender, RoutedEventArgs e)
    {
        var browser = PalettesBrowser.Open(PaletteProvider, ImportPaletteCommand, Colors);
        await browser.UpdatePaletteList();
    }

    private void ReplaceColor_OnClick(object sender, RoutedEventArgs e)
    {
        MenuItem menuItem = (MenuItem)sender;
        PaletteColor color = (PaletteColor)menuItem.CommandParameter;
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

    private void DiscardPalette_OnClick(object sender, RoutedEventArgs e)
    {
        if(ConfirmationDialog.Show("DISCARD_PALETTE_CONFIRMATION", "DISCARD_PALETTE") == ConfirmationType.Yes)
        {
            Colors.Clear();
        }
    }
}
