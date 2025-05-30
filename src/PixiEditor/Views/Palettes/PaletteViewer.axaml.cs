using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Extensions.CommonApi.Palettes.Parsers;
using PixiEditor.Helpers;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.ExtensionServices;
using PixiEditor.Models.IO.PaletteParsers;
using PixiEditor.Models.Structures;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels;
using PixiEditor.Views.Dialogs;
using PixiEditor.Views.Windows;

namespace PixiEditor.Views.Palettes;

/// <summary>
/// Interaction logic for Palette.xaml
/// </summary>
internal partial class PaletteViewer : UserControl
{
    public static readonly StyledProperty<ObservableCollection<PaletteColor>> SwatchesProperty =
        AvaloniaProperty.Register<PaletteViewer, ObservableCollection<PaletteColor>>(
            nameof(Swatches));

    public ObservableCollection<PaletteColor> Swatches
    {
        get => GetValue(SwatchesProperty);
        set => SetValue(SwatchesProperty, value);
    }

    public static readonly StyledProperty<ObservableRangeCollection<PaletteColor>> ColorsProperty =
        AvaloniaProperty.Register<PaletteViewer, ObservableRangeCollection<PaletteColor>>(
            nameof(Colors));

    public ObservableRangeCollection<PaletteColor> Colors
    {
        get => GetValue(ColorsProperty);
        set => SetValue(ColorsProperty, value);
    }

    public static readonly StyledProperty<Color> HintColorProperty =
        AvaloniaProperty.Register<PaletteViewer, Color>(
            nameof(HintColor),
            default(Color));

    public Color HintColor
    {
        get => GetValue(HintColorProperty);
        set => SetValue(HintColorProperty, value);
    }

    public static readonly StyledProperty<ICommand> ReplaceColorsCommandProperty =
        AvaloniaProperty.Register<PaletteViewer, ICommand>(
            nameof(ReplaceColorsCommand),
            default(ICommand));

    public ICommand ReplaceColorsCommand
    {
        get => GetValue(ReplaceColorsCommandProperty);
        set => SetValue(ReplaceColorsCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand> SelectColorCommandProperty =
        AvaloniaProperty.Register<PaletteViewer, ICommand>(
            nameof(SelectColorCommand));

    public ICommand SelectColorCommand
    {
        get => GetValue(SelectColorCommandProperty);
        set => SetValue(SelectColorCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand> ImportPaletteCommandProperty =
        AvaloniaProperty.Register<PaletteViewer, ICommand>(
            nameof(ImportPaletteCommand));

    public ICommand ImportPaletteCommand
    {
        get => GetValue(ImportPaletteCommandProperty);
        set => SetValue(ImportPaletteCommandProperty, value);
    }

    public static readonly StyledProperty<PaletteProvider> PaletteProviderProperty =
        AvaloniaProperty.Register<PaletteViewer, PaletteProvider>(
            nameof(PaletteProvider),
            default(PaletteProvider));

    public PaletteProvider PaletteProvider
    {
        get => GetValue(PaletteProviderProperty);
        set => SetValue(PaletteProviderProperty, value);
    }

    public static readonly StyledProperty<bool> IsCompactProperty = AvaloniaProperty.Register<PaletteViewer, bool>(
        nameof(IsCompact),
        false);

    public bool IsCompact
    {
        get => GetValue(IsCompactProperty);
        set => SetValue(IsCompactProperty, value);
    }
    
    public ICommand DropColorCommand { get; set; }

    public PaletteViewer()
    {
        InitializeComponent();
        SizeChanged += OnSizeChanged;
        
        MainDropTarget.AddHandler(DragDrop.DragEnterEvent, Grid_PreviewDragEnter);
        MainDropTarget.AddHandler(DragDrop.DragLeaveEvent, Grid_PreviewDragLeave);
        MainDropTarget.AddHandler(DragDrop.DropEvent, Grid_Drop);

        DropColorCommand = new RelayCommand<DragEventArgs>(PaletteColor_Drop);
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        IsCompact = e.NewSize.Width < 240 || e.NewSize.Height < 240;
    }

    private void RemoveColorMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        MenuItem menuItem = (MenuItem)sender;
        PaletteColor colorControl = (PaletteColor)menuItem.CommandParameter;
        if (Colors.Contains(colorControl))
        {
            Colors.Remove(colorControl);
            RefreshAllItems();
        }
    }

    private async void ImportPalette_OnClick(object sender, RoutedEventArgs e)
    {
        await Application.Current.ForDesktopMainWindowAsync(async window =>
        {
            var file = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                FileTypeFilter = PaletteHelpers.GetFilter(PaletteProvider.AvailableParsers, true),
            });

            if (file is null || file.Count == 0) return;

            await ImportPalette(file[0].Path.LocalPath);
        });
    }

    private async Task ImportPalette(string filePath)
    {
        // check if valid parser found
        var parser = await PaletteHelpers.GetValidParser(PaletteProvider.AvailableParsers, filePath);
        if (parser != null)
        {
            Colors.Clear();
            Colors.AddRange(parser.Colors);
        }
    }

    private async void SavePalette_OnClick(object sender, RoutedEventArgs e)
    {
        await Application.Current.ForDesktopMainWindowAsync(async window =>
        {
            List<PaletteFileParser> availableParsers = PaletteProvider.AvailableParsers.Where(x => x.CanSave).ToList();
            var file = await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
            {
                FileTypeChoices = PaletteHelpers.GetFilter(availableParsers, false),
                SuggestedFileName = new LocalizedString("NEW_PALETTE_FILE"),
            });

            if (file is null) return;
            
            string extension = Path.GetExtension(file.Path.LocalPath);

            var parsersOfExtensions =
                availableParsers.Where(x => x.SupportedFileExtensions.Contains(extension)).ToList();
            
            var foundParser = await GetPaletteFileParser(parsersOfExtensions);

            if (foundParser is null)
            {
                NoticeDialog.Show(new LocalizedString("NO_PARSER_FOUND", extension), "ERROR");
                return;
            }
            
            if (Colors is null || Colors.Count == 0)
            {
                NoticeDialog.Show("NO_COLORS_TO_SAVE", "ERROR");
                return;
            }

            try
            {
                bool saved = await foundParser.Save(file.Path.LocalPath, new PaletteFileData(Colors.ToArray()));
                if (!saved)
                {
                    NoticeDialog.Show("COULD_NOT_SAVE_PALETTE", "ERROR");
                }
            }
            catch (SavingNotSupportedException savingNotSupportedException)
            {
                NoticeDialog.Show("COULD_NOT_SAVE_PALETTE", "ERROR");
            }
            catch (UnauthorizedAccessException unauthorizedAccessException)
            {
                NoticeDialog.Show(new LocalizedString("UNAUTHORIZED_ACCESS", file.Path.LocalPath), "ERROR");
            }
            catch (Exception ex)
            {
                NoticeDialog.Show(new LocalizedString("ERROR_SAVING_PALETTE", ex.Message), "ERROR");
            }
        });
    }

    private static async Task<PaletteFileParser?> GetPaletteFileParser(List<PaletteFileParser> parsersOfExtensions)
    {
        PaletteFileParser foundParser = null;

        if (parsersOfExtensions.Count > 1)
        {
            var optionsDialog = new OptionsDialog<PaletteFileParser>(
                new LocalizedString("SELECT_FILE_FORMAT"),
                new LocalizedString("SELECT_FILE_FORMAT_DESCRIPTION"), MainWindow.Current);

            foreach (var pars in parsersOfExtensions)
            {
                optionsDialog.Add(pars);
            }
                
            if (await optionsDialog.ShowDialog())
            {
                foundParser = optionsDialog.Result;
            }
        }
        else if (parsersOfExtensions.Count == 1)
        {
            foundParser = parsersOfExtensions.First();
        }

        return foundParser;
    }

    private void Grid_PreviewDragEnter(object sender, DragEventArgs e)
    {
        if (IsSupportedFilePresent(e, out _))
        {
            dragDropGrid.IsVisible = true;
            ViewModelMain.Current.ActionDisplays[nameof(PaletteViewer)] = "IMPORT_PALETTE_FILE";
        }
        else if (ColorHelper.ParseAnyFormatList(e.Data, out var list))
        {
            e.DragEffects = DragDropEffects.Copy;
            ViewModelMain.Current.ActionDisplays[nameof(PaletteViewer)] =
                list.Count > 1 ? "IMPORT_MULTIPLE_PALETTE_COLORS" : "IMPORT_SINGLE_PALETTE_COLOR";
            e.Handled = true;
        }
    }

    private void Grid_PreviewDragLeave(object sender, DragEventArgs e)
    {
        dragDropGrid.IsVisible = false;
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

            e.DragEffects = DragDropEffects.Copy;
            Colors.AddRange(paletteColors.Where(x => !Colors.Contains(new PaletteColor(x.R, x.G, x.B))).ToList());
            e.Handled = true;
            return;
        }

        e.Handled = true;
        await ImportPalette(filePath);
        dragDropGrid.IsVisible = false;
    }

    private bool IsSupportedFilePresent(DragEventArgs e, out string? filePath)
    {
        if (e.Data.Contains(DataFormats.Files))
        {
            IStorageItem[]? files = e.Data.GetFiles()?.ToArray();
            if (files is null)
            {
                filePath = null;
                return false;
            }

            if (files is { Length: > 0 })
            {
                IStorageItem file = files[0];
                var foundParser = PaletteProvider.AvailableParsers.FirstOrDefault(x =>
                    x.SupportedFileExtensions.Contains(Path.GetExtension(file.Path.LocalPath)));
                if (foundParser != null)
                {
                    filePath = file.Path.LocalPath;
                    return true;
                }
            }
        }

        filePath = null;
        return false;
    }

    private void PaletteColor_Drop( DragEventArgs e)
    {
        if (e.Data.Contains(PaletteColorControl.PaletteColorDaoFormat))
        {
            string data = (string)e.Data.Get(PaletteColorControl.PaletteColorDaoFormat);

            PaletteColor paletteColor = PaletteColor.Parse(data);
            if (Colors.Contains(paletteColor))
            {
                PaletteColorControl paletteColorControl = e.Source as PaletteColorControl;
                int currIndex = Colors.IndexOf(paletteColor);
                if (paletteColorControl != null)
                {
                    int newIndex = Colors.IndexOf(paletteColorControl.Color);
                    Colors.RemoveAt(currIndex);
                    Colors.Insert(newIndex, paletteColor);
                    int indexOfSource = Colors.IndexOf(paletteColorControl.Color);
                    if(indexOfSource < 0) return;
                    Colors.Move(indexOfSource, currIndex);
                }
            }
        }
    }

    private void RefreshAllItems()
    {
        PaletteItemsSource.ItemsSource = null;
        PaletteItemsSource.ItemsSource = Colors;
    }

    private async void BrowsePalettes_Click(object sender, RoutedEventArgs e)
    {
        var browser = PalettesBrowser.Open();
        await browser.UpdatePaletteList();
    }

    private void ReplaceColor_OnClick(object sender, RoutedEventArgs e)
    {
        MenuItem menuItem = (MenuItem)sender;
        PaletteColor color = (PaletteColor)menuItem.CommandParameter;
        Replacer.ColorToReplace = color;
        Replacer.Shelf.IsOpen = true;
    }

    private void MenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        MenuItem origin = (MenuItem)sender;
        if (SelectColorCommand.CanExecute(origin.CommandParameter))
        {
            SelectColorCommand.Execute(origin.CommandParameter);
        }
    }

    private async void DiscardPalette_OnClick(object sender, RoutedEventArgs e)
    {
        if (await ConfirmationDialog.Show("DISCARD_PALETTE_CONFIRMATION", "DISCARD_PALETTE") == ConfirmationType.Yes)
        {
            Colors.Clear();
        }
    }
}
