using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Microsoft.Win32;
using PixiEditor.Helpers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.DataHolders.Palettes;
using PixiEditor.Models.DataProviders;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Enums;
using PixiEditor.Models.IO;
using PixiEditor.Models.UserPreferences;
using PixiEditor.ViewModels;
using PixiEditor.Views.UserControls.Palettes;
using SkiaSharp;

namespace PixiEditor.Views.Dialogs;

internal partial class PalettesBrowser : Window
{
    private const int ItemsPerLoad = 10;

    private readonly string[] stopItTexts = new[]
    {
        "That's enough. Tidy up your file names.",
        "Can you stop copying these names please?", "No, really, stop it.", "Don't you have anything better to do?"
    };

    public PaletteList PaletteList
    {
        get => (PaletteList)GetValue(PaletteListProperty);
        set => SetValue(PaletteListProperty, value);
    }

    public static readonly DependencyProperty PaletteListProperty =
        DependencyProperty.Register(nameof(PaletteList), typeof(PaletteList), typeof(PalettesBrowser));

    public ICommand ImportPaletteCommand
    {
        get => (ICommand)GetValue(ImportPaletteCommandProperty);
        set => SetValue(ImportPaletteCommandProperty, value);
    }

    public static readonly DependencyProperty ImportPaletteCommandProperty =
        DependencyProperty.Register(nameof(ImportPaletteCommand), typeof(ICommand), typeof(PalettesBrowser));

    public static readonly DependencyProperty DeletePaletteCommandProperty = DependencyProperty.Register(
        nameof(DeletePaletteCommand), typeof(ICommand), typeof(PalettesBrowser), new PropertyMetadata(default(ICommand)));

    public ICommand DeletePaletteCommand
    {
        get => (ICommand)GetValue(DeletePaletteCommandProperty);
        set => SetValue(DeletePaletteCommandProperty, value);
    }

    public bool IsFetching
    {
        get => (bool)GetValue(IsFetchingProperty);
        set => SetValue(IsFetchingProperty, value);
    }

    public static readonly DependencyProperty IsFetchingProperty =
        DependencyProperty.Register(nameof(IsFetching), typeof(bool), typeof(PalettesBrowser), new PropertyMetadata(false));

    public int ColorsNumber
    {
        get => (int)GetValue(ColorsNumberProperty);
        set => SetValue(ColorsNumberProperty, value);
    }

    public static readonly DependencyProperty ColorsNumberProperty =
        DependencyProperty.Register(nameof(ColorsNumber), typeof(int), typeof(PalettesBrowser),
            new PropertyMetadata(8, ColorsNumberChanged));

    public WpfObservableRangeCollection<PaletteListDataSource> PaletteListDataSources
    {
        get => (WpfObservableRangeCollection<PaletteListDataSource>)GetValue(PaletteListDataSourcesProperty);
        set => SetValue(PaletteListDataSourcesProperty, value);
    }

    public static readonly DependencyProperty PaletteListDataSourcesProperty =
        DependencyProperty.Register(nameof(PaletteListDataSources), typeof(WpfObservableRangeCollection<PaletteListDataSource>), typeof(PalettesBrowser), new PropertyMetadata(new WpfObservableRangeCollection<PaletteListDataSource>()));
    public bool SortAscending
    {
        get => (bool)GetValue(SortAscendingProperty);
        set => SetValue(SortAscendingProperty, value);
    }

    public static readonly DependencyProperty SortAscendingProperty =
        DependencyProperty.Register(nameof(SortAscending), typeof(bool), typeof(PalettesBrowser), new PropertyMetadata(true, OnSortAscendingChanged));


    public static readonly DependencyProperty SortedResultsProperty = DependencyProperty.Register(
        nameof(SortedResults), typeof(WpfObservableRangeCollection<Palette>), typeof(PalettesBrowser), new PropertyMetadata(default(WpfObservableRangeCollection<Palette>)));

    public WpfObservableRangeCollection<Palette> SortedResults
    {
        get => (WpfObservableRangeCollection<Palette>)GetValue(SortedResultsProperty);
        set => SetValue(SortedResultsProperty, value);
    }

    public static readonly DependencyProperty NameFilterProperty = DependencyProperty.Register(
        nameof(NameFilter), typeof(string), typeof(PalettesBrowser),
        new PropertyMetadata(default(string), OnNameFilterChanged));

    public string NameFilter
    {
        get => (string)GetValue(NameFilterProperty);
        set => SetValue(NameFilterProperty, value);
    }

    public static readonly DependencyProperty ShowOnlyFavouritesProperty = DependencyProperty.Register(
        nameof(ShowOnlyFavourites), typeof(bool), typeof(PalettesBrowser),
        new PropertyMetadata(false, OnShowOnlyFavouritesChanged));

    public bool ShowOnlyFavourites
    {
        get => (bool)GetValue(ShowOnlyFavouritesProperty);
        set => SetValue(ShowOnlyFavouritesProperty, value);
    }

    public RelayCommand<Palette> ToggleFavouriteCommand { get; set; }

    public string SortingType { get; set; } = "Default";
    public ColorsNumberMode ColorsNumberMode { get; set; } = ColorsNumberMode.Any;

    private FilteringSettings filteringSettings;

    public FilteringSettings Filtering => filteringSettings ??=
        new FilteringSettings(ColorsNumberMode, ColorsNumber, NameFilter, ShowOnlyFavourites);

    private char[] separators = new char[] { ' ', ',' };

    private SortingType InternalSortingType => (SortingType)Enum.Parse(typeof(SortingType), SortingType.Replace(" ", ""));
    public WpfObservableRangeCollection<SKColor> CurrentEditingPalette { get; set; }
    public static PalettesBrowser Instance { get; internal set; }

    private LocalPalettesFetcher LocalPalettesFetcher
    {
        get
        {
            return localPalettesFetcher ??= (LocalPalettesFetcher)PaletteListDataSources.First(x => x is LocalPalettesFetcher);
        }
    }

    private LocalPalettesFetcher localPalettesFetcher;

    public PalettesBrowser()
    {
        InitializeComponent();
        Instance = this;
        DeletePaletteCommand = new RelayCommand<Palette>(DeletePalette);
        ToggleFavouriteCommand = new RelayCommand<Palette>(ToggleFavourite, CanToggleFavourite);
        Loaded += async (_, _) =>
        {
            LocalPalettesFetcher.CacheUpdated += LocalCacheRefreshed;
            await LocalPalettesFetcher.RefreshCacheAll();
        };
        Closed += (_, _) =>
        {
            Instance = null;
            LocalPalettesFetcher.CacheUpdated -= LocalCacheRefreshed;
        };
    }

    public static PalettesBrowser Open(WpfObservableRangeCollection<PaletteListDataSource> dataSources, ICommand importPaletteCommand, WpfObservableRangeCollection<SKColor> currentEditingPalette)
    {
        if (Instance != null) return Instance;
        PalettesBrowser browser = new PalettesBrowser
        {
            Owner = Application.Current.MainWindow,
            ImportPaletteCommand = importPaletteCommand,
            PaletteListDataSources = dataSources,
            CurrentEditingPalette = currentEditingPalette
        };

        browser.Show();
        return browser;
    }

    private async void LocalCacheRefreshed(RefreshType refreshType, Palette itemAffected, string fileNameAffected)
    {
        await Dispatcher.InvokeAsync(async () =>
        {
            switch (refreshType)
            {
                case RefreshType.All:
                    await UpdatePaletteList();
                    break;
                case RefreshType.Created:
                    HandleCachePaletteCreated(itemAffected);
                    break;
                case RefreshType.Updated:
                    HandleCacheItemUpdated(itemAffected);
                    break;
                case RefreshType.Deleted:
                    HandleCacheItemDeleted(fileNameAffected);
                    break;
                case RefreshType.Renamed:
                    HandleCacheItemRenamed(itemAffected, fileNameAffected);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(refreshType), refreshType, null);
            }

        });
    }

    private void HandleCacheItemRenamed(Palette itemAffected, string oldFileName)
    {
        var old = SortedResults.FirstOrDefault(x => x.FileName == oldFileName);
        if (old != null)
        {
            old.Name = itemAffected.Name;
            old.FileName = itemAffected.FileName;
        }

        UpdateRenamedFavourite(Path.GetFileNameWithoutExtension(oldFileName), itemAffected.Name);
        Sort();
    }

    private void HandleCacheItemDeleted(string deletedItemFileName)
    {
        Palette item = SortedResults.FirstOrDefault(x => x.FileName == deletedItemFileName);
        if (item != null)
        {
            SortedResults.Remove(item);
            PaletteList.Palettes.Remove(item);
        }
    }

    private void HandleCacheItemUpdated(Palette updatedItem)
    {
        var item = SortedResults.FirstOrDefault(x => x.FileName == updatedItem.FileName);
        if (item is null)
            return;

        item.Name = updatedItem.Name;
        item.IsFavourite = updatedItem.IsFavourite;
        item.Colors = updatedItem.Colors;

        Sort();
    }

    private void HandleCachePaletteCreated(Palette updatedItem)
    {
        SortedResults.Add(updatedItem);
        PaletteList.Palettes.Add(updatedItem);
        Sort();
    }

    private async void ToggleFavourite(Palette palette)
    {
        palette.IsFavourite = !palette.IsFavourite;
        var favouritePalettes = IPreferences.Current.GetLocalPreference(PreferencesConstants.FavouritePalettes, new List<string>());

        if (palette.IsFavourite)
        {
            favouritePalettes.Add(palette.Name);
        }
        else
        {
            favouritePalettes.Remove(palette.Name);
        }

        IPreferences.Current.UpdateLocalPreference(PreferencesConstants.FavouritePalettes, favouritePalettes);
        await UpdatePaletteList();
    }

    private void DeletePalette(Palette palette)
    {
        if (palette == null) return;

        string filePath = Path.Join(LocalPalettesFetcher.PathToPalettesFolder, palette.FileName);
        if (File.Exists(filePath))
        {
            if (ConfirmationDialog.Show("Are you sure you want to delete this palette? This cannot be undone.", "Warning!") == ConfirmationType.Yes)
            {
                _ = LocalPalettesFetcher.DeletePalette(palette.FileName);
                RemoveFavouritePalette(palette);
            }
        }
    }

    private static void RemoveFavouritePalette(Palette palette)
    {
        var favouritePalettes =
            IPreferences.Current.GetLocalPreference<List<string>>(PreferencesConstants.FavouritePalettes);
        if (favouritePalettes != null && favouritePalettes.Contains(palette.Name))
        {
            favouritePalettes.Remove(palette.Name);
            IPreferences.Current.UpdateLocalPreference(PreferencesConstants.FavouritePalettes, favouritePalettes);
        }
    }

    private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private static async void OnShowOnlyFavouritesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        PalettesBrowser browser = (PalettesBrowser)d;
        browser.Filtering.ShowOnlyFavourites = (bool)e.NewValue;
        await browser.UpdatePaletteList();
    }

    private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
    {
        SystemCommands.CloseWindow(this);
    }

    private static async void ColorsNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        PalettesBrowser browser = (PalettesBrowser)d;
        browser.Filtering.ColorsCount = (int)e.NewValue;
        await browser.UpdatePaletteList();
    }

    private static void OnSortAscendingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        PalettesBrowser browser = (PalettesBrowser)d;
        browser.Sort();
    }

    public async Task UpdatePaletteList()
    {
        IsFetching = true;
        PaletteList?.Palettes?.Clear();

        for (int i = 0; i < PaletteListDataSources.Count; i++)
        {
            PaletteList src = await FetchPaletteList(i, Filtering);
            if (!src.FetchedCorrectly) continue;
            if (PaletteList == null)
            {
                PaletteList = src;
            }
            else
            {
                PaletteList.Palettes?.AddRange(src.Palettes);
            }
        }

        Sort();

        IsFetching = false;
    }

    private async Task<PaletteList> FetchPaletteList(int index, FilteringSettings filtering)
    {
        int startIndex = PaletteList != null ? PaletteList.Palettes.Count : 0;
        var src = await PaletteListDataSources[index].FetchPaletteList(startIndex, ItemsPerLoad, filtering);
        return src;
    }

    private static async void OnNameFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var browser = (PalettesBrowser)d;
        browser.Filtering.Name = browser.NameFilter;
        await browser.UpdatePaletteList();
        browser.scrollViewer.ScrollToHome();
    }

    private async void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (PaletteList?.Palettes == null) return;
        var viewer = (ScrollViewer)sender;
        if (viewer.VerticalOffset == viewer.ScrollableHeight)
        {
            IsFetching = true;
            var newPalettes = await FetchPaletteList(0, Filtering);
            if (newPalettes is not { FetchedCorrectly: true } || newPalettes.Palettes == null)
            {
                IsFetching = false;
                return;
            }

            PaletteList.Palettes.AddRange(newPalettes.Palettes);
            Sort();
            IsFetching = false;
        }
    }

    private void SortingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems is { Count: > 0 } && e.AddedItems[0] is ComboBoxItem { Content: string value })
        {
            SortingType = value;
            Sort();
            scrollViewer.ScrollToHome();
        }
    }

    private async void ColorsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems is { Count: > 0 } && e.AddedItems[0] is ComboBoxItem { Content: string value })
        {
            ColorsNumberMode = Enum.Parse<ColorsNumberMode>(value);
            Filtering.ColorsNumberMode = ColorsNumberMode;
            await UpdatePaletteList();
            scrollViewer.ScrollToHome();
        }
    }

    private bool CanToggleFavourite(Palette palette)
    {
        return palette != null && palette.Colors.Count > 0;
    }

    private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
    {
        ((Grid)sender).Focus();
    }

    private void Sort()
    {
        Sort(!SortAscending);
    }

    private void Sort(bool descending)
    {
        if (PaletteList?.Palettes == null) return;

        IOrderedEnumerable<Palette> sorted = null;
        if (!descending)
        {
            switch (InternalSortingType)
            {
                case Models.DataHolders.Palettes.SortingType.Default:
                    sorted = PaletteList.Palettes.OrderByDescending(x => x.IsFavourite).ThenBy(x => PaletteList.Palettes.IndexOf(x));
                    break;
                case Models.DataHolders.Palettes.SortingType.Alphabetical:
                    sorted = PaletteList.Palettes.OrderBy(x => x.Name);
                    break;
                case Models.DataHolders.Palettes.SortingType.ColorCount:
                    sorted = PaletteList.Palettes.OrderBy(x => x.Colors.Count);
                    break;
            }
        }
        else
        {
            switch (InternalSortingType)
            {
                case Models.DataHolders.Palettes.SortingType.Default:
                    sorted = PaletteList.Palettes.OrderByDescending(x => PaletteList.Palettes.IndexOf(x));
                    break;
                case Models.DataHolders.Palettes.SortingType.Alphabetical:
                    sorted = PaletteList.Palettes.OrderByDescending(x => x.Name);
                    break;
                case Models.DataHolders.Palettes.SortingType.ColorCount:
                    sorted = PaletteList.Palettes.OrderByDescending(x => x.Colors.Count);
                    break;
            }
        }

        if (sorted != null)
        {
            SortedResults = new WpfObservableRangeCollection<Palette>(sorted);
        }
    }

    private void OpenFolder_OnClick(object sender, RoutedEventArgs e)
    {
        if (Directory.Exists(LocalPalettesFetcher.PathToPalettesFolder))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = LocalPalettesFetcher.PathToPalettesFolder,
                UseShellExecute = true,
                Verb = "open"
            });
        }
    }

    private async void AddFromPalette_OnClick(object sender, RoutedEventArgs e)
    {
        if (CurrentEditingPalette?.Count == 0)
            return;

        string finalFileName = LocalPalettesFetcher.GetNonExistingName("Unnamed Palette.pal", true);
        await LocalPalettesFetcher.SavePalette(finalFileName, CurrentEditingPalette.ToArray());
    }

    private void PaletteItem_OnRename(object sender, EditableTextBlock.TextChangedEventArgs e)
    {
        PaletteItem item = (PaletteItem)sender;
        item.Palette.Name = e.OldText;

        if (string.IsNullOrWhiteSpace(e.NewText) || e.NewText == item.Palette.Name || e.NewText.Length > 50)
            return;

        string oldFileName = $"{e.OldText}.pal";

        string finalNewName = LocalPalettesFetcher.GetNonExistingName($"{Palette.ReplaceInvalidChars(e.NewText)}.pal", true);
        string newPath = Path.Join(LocalPalettesFetcher.PathToPalettesFolder, finalNewName);

        if (newPath.Length > 250)
        {
            NoticeDialog.Show(stopItTexts[Random.Shared.Next(stopItTexts.Length - 1)], "The name is too long.");
            return;
        }

        LocalPalettesFetcher.RenamePalette(oldFileName, finalNewName);
    }

    private static void UpdateRenamedFavourite(string old, string newName)
    {
        var favourites = IPreferences.Current.GetLocalPreference(
            PreferencesConstants.FavouritePalettes,
            new List<string>());

        if (favourites.Contains(old))
        {
            favourites.Remove(old);
            favourites.Add(newName);
        }

        IPreferences.Current.UpdateLocalPreference(PreferencesConstants.FavouritePalettes, favourites);
    }

    private void BrowseOnLospec_OnClick(object sender, RoutedEventArgs e)
    {
        Button button = sender as Button;
        string url = (string)button.CommandParameter;

        ProcessHelpers.ShellExecute(url);
    }


    private async void ImportFromFile_OnClick(object sender, RoutedEventArgs e)
    {
        var parsers = ViewModelMain.Current.ColorsSubViewModel.PaletteParsers;
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Filter = PaletteHelpers.GetFilter(parsers, true)
        };

        if (openFileDialog.ShowDialog() == true)
        {
            await ImportPalette(openFileDialog.FileName, parsers);
        }
    }

    private async Task ImportPalette(string fileName, IList<PaletteFileParser> parsers)
    {
        var parser = parsers.FirstOrDefault(x => x.SupportedFileExtensions.Contains(Path.GetExtension(fileName)));
        if (parser != null)
        {
            var data = await parser.Parse(fileName);

            if (data.IsCorrupted) return;
            string name = LocalPalettesFetcher.GetNonExistingName(Path.GetFileName(fileName), true);
            await LocalPalettesFetcher.SavePalette(name, data.Colors.ToArray());
        }
    }

    private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        ProcessHelpers.ShellExecute(e.Uri.ToString());
    }
}
