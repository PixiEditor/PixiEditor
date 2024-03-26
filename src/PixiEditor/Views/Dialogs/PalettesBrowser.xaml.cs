using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Microsoft.Win32;
using PixiEditor.Extensions;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.Common.UserPreferences;
using PixiEditor.Extensions.Palettes;
using PixiEditor.Extensions.Palettes.Parsers;
using PixiEditor.Extensions.Windowing;
using PixiEditor.Helpers;
using PixiEditor.Models.AppExtensions.Services;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.DataHolders.Palettes;
using PixiEditor.Models.DataProviders;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Enums;
using PixiEditor.Models.IO;
using PixiEditor.Views.UserControls;
using PixiEditor.Views.UserControls.Palettes;
using PaletteColor = PixiEditor.Extensions.Palettes.PaletteColor;

namespace PixiEditor.Views.Dialogs;

internal partial class PalettesBrowser : Window, IPopupWindow
{
    public static string UniqueId => "PixiEditor.BrowserPalette";
    string IPopupWindow.UniqueId => UniqueId;

    private const int ItemsPerLoad = 25;

    private readonly LocalizedString[] stopItTexts = new[]
    {
        new LocalizedString("STOP_IT_TEXT1"),
        new LocalizedString("STOP_IT_TEXT2"),
        new LocalizedString("STOP_IT_TEXT3"),
        new LocalizedString("STOP_IT_TEXT4"),
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

    public static readonly DependencyProperty AddFromPaletteCommandProperty = DependencyProperty.Register(
        nameof(AddFromPaletteCommand), typeof(ICommand), typeof(PalettesBrowser), new PropertyMetadata(default(ICommand)));

    public ICommand AddFromPaletteCommand
    {
        get { return (ICommand)GetValue(AddFromPaletteCommandProperty); }
        set { SetValue(AddFromPaletteCommandProperty, value); }
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

    public static readonly DependencyProperty PaletteProviderProperty = DependencyProperty.Register(
        nameof(PaletteProvider), typeof(PaletteProvider), typeof(PalettesBrowser), new PropertyMetadata(default(PaletteProvider)));

    public PaletteProvider PaletteProvider
    {
        get { return (PaletteProvider)GetValue(PaletteProviderProperty); }
        set { SetValue(PaletteProviderProperty, value); }
    }
    public RelayCommand<Palette> ToggleFavouriteCommand { get; set; }
    public int SortingIndex { get; set; } = 0;
    public ColorsNumberMode ColorsNumberMode { get; set; } = ColorsNumberMode.Any;

    private FilteringSettings filteringSettings;

    public FilteringSettings Filtering => filteringSettings ??=
        new FilteringSettings(ColorsNumberMode, ColorsNumber, NameFilter, ShowOnlyFavourites,
            IPreferences.Current.GetLocalPreference<List<string>>(PreferencesConstants.FavouritePalettes, new List<string>()));

    private char[] separators = new char[] { ' ', ',' };

    private SortingType InternalSortingType => (SortingType)SortingIndex;
    public WpfObservableRangeCollection<PaletteColor> CurrentEditingPalette { get; set; }
    public static PalettesBrowser Instance { get; internal set; }

    private LocalPalettesFetcher LocalPalettesFetcher
    {
        get
        {
            return localPalettesFetcher ??= (LocalPalettesFetcher)PaletteProvider.DataSources.First(x => x is LocalPalettesFetcher);
        }
    }

    private LocalPalettesFetcher localPalettesFetcher;

    private ILocalizationProvider localizationProvider;

    private double _lastScrolledOffset = -1;

    public PalettesBrowser(PaletteProvider provider)
    {
        localizationProvider = ViewModelMain.Current.LocalizationProvider;
        localizationProvider.OnLanguageChanged += LocalizationProviderOnOnLanguageChanged;
        MinWidth = DetermineWidth();

        PaletteProvider = provider;
        InitializeComponent();
        Title = new LocalizedString("PALETTE_BROWSER");
        Instance = this;

        DeletePaletteCommand = new RelayCommand<Palette>(DeletePalette, CanDeletePalette);
        ToggleFavouriteCommand = new RelayCommand<Palette>(ToggleFavourite, CanToggleFavourite);
        AddFromPaletteCommand = new RelayCommand(AddFromCurrentPalette, CanAddFromPalette);
        Loaded += async (_, _) =>
        {
            await UpdatePaletteList();
            LocalPalettesFetcher.CacheUpdated += LocalCacheRefreshed;
        };
        Closed += (_, _) =>
        {
            Instance = null;
            LocalPalettesFetcher.CacheUpdated -= LocalCacheRefreshed;
        };

        IPreferences.Current.AddCallback(PreferencesConstants.FavouritePalettes, OnFavouritePalettesChanged);
    }

    private void LocalizationProviderOnOnLanguageChanged(Language obj)
    {
        MinWidth = DetermineWidth();
    }

    private double DetermineWidth()
    {
        return localizationProvider.CurrentLanguage.LanguageData.Code switch
        {
            "ru" or "uk" => 900,
            _ => 850
        };
    }

    private bool CanAddFromPalette(object param)
    {
        return CurrentEditingPalette != null;
    }

    private bool CanDeletePalette(Palette palette)
    {
        return palette != null && palette.Source.GetType() == typeof(LocalPalettesFetcher);
    }

    private void OnFavouritePalettesChanged(object obj)
    {
        Filtering.Favourites =
            IPreferences.Current.GetLocalPreference<List<string>>(PreferencesConstants.FavouritePalettes);
    }

    public static PalettesBrowser Open(PaletteProvider provider, ICommand importPaletteCommand, WpfObservableRangeCollection<PaletteColor> currentEditingPalette)
    {
        if (Instance != null) return Instance;
        PalettesBrowser browser = new PalettesBrowser(provider)
        {
            Owner = Application.Current.MainWindow,
            ImportPaletteCommand = importPaletteCommand,
            CurrentEditingPalette = currentEditingPalette
        };

        browser.Show();
        return browser;
    }

    private async void LocalCacheRefreshed(RefreshType refreshType, Palette itemAffected, string fileNameAffected)
    {
        await Dispatcher.InvokeAsync(async () =>
        {
            SortedResults ??= new WpfObservableRangeCollection<Palette>();
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

        if (palette.IsFavourite && !favouritePalettes.Contains(palette.Name))
        {
            favouritePalettes.Add(palette.Name);
        }
        else
        {
            favouritePalettes.RemoveAll(x => x == palette.Name);
        }

        IPreferences.Current.UpdateLocalPreference(PreferencesConstants.FavouritePalettes, favouritePalettes);
        await UpdatePaletteList();
    }

    private bool IsPaletteFavourite(string name)
    {
        var favouritePalettes = IPreferences.Current.GetLocalPreference(PreferencesConstants.FavouritePalettes, new List<string>());
        return favouritePalettes.Contains(name);
    }

    private void DeletePalette(Palette palette)
    {
        if (palette == null) return;

        string filePath = Path.Join(Paths.PathToPalettesFolder, palette.FileName);
        if (File.Exists(filePath))
        {
            if (ConfirmationDialog.Show("DELETE_PALETTE_CONFIRMATION", "WARNING") == ConfirmationType.Yes)
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
        _lastScrolledOffset = -1;
        PaletteList?.Palettes?.Clear();
        PaletteList src = await FetchPaletteList(Filtering);
        if (PaletteList == null)
        {
            PaletteList = src;
        }
        else
        {
            AddToPaletteList(src.Palettes);
        }

        Sort();

        IsFetching = false;
    }

    private void AddToPaletteList(WpfObservableRangeCollection<Palette> srcPalettes)
    {
        if (srcPalettes == null)
            return;

        foreach (var pal in srcPalettes)
        {
            if (PaletteEquals(pal, PaletteList.Palettes)) continue;
            PaletteList.Palettes.Add(pal);
        }
    }

    private async Task<PaletteList> FetchPaletteList(FilteringSettings filtering)
    {
        int startIndex = PaletteList != null ? PaletteList.Palettes.Count : 0;
        var src = await PaletteProvider.FetchPalettes(startIndex, ItemsPerLoad, filtering);
        WpfObservableRangeCollection<Palette> palettes = new WpfObservableRangeCollection<Palette>();
        if (src != null)
        {
            foreach (var pal in src)
            {
                palettes.Add(new Palette(pal.Name, pal.Colors, pal.FileName, pal.Source) { IsFavourite = IsPaletteFavourite(pal.Name) });
            }
        }

        PaletteList list = new PaletteList { Palettes = palettes, FetchedCorrectly = src != null };
        return list;
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
        if (viewer.VerticalOffset == viewer.ScrollableHeight && _lastScrolledOffset != viewer.VerticalOffset)
        {
            IsFetching = true;
            var newPalettes = await FetchPaletteList(Filtering);
            if (newPalettes is not { FetchedCorrectly: true } || newPalettes.Palettes == null)
            {
                IsFetching = false;
                return;
            }

            AddToPaletteList(newPalettes.Palettes);
            Sort();
            IsFetching = false;

            _lastScrolledOffset = viewer.VerticalOffset;
        }
    }

    private bool PaletteEquals(Palette palette, WpfObservableRangeCollection<Palette> paletteListPalettes)
    {
        return paletteListPalettes.Any(x => x.Name == palette.Name && x.Source == palette.Source && x.Colors.Count == palette.Colors.Count
        && x.Colors.SequenceEqual(palette.Colors));
    }

    private void SortingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems is { Count: > 0 } && e.AddedItems[0] is ComboBoxItem)
        {
            var comboBox = (ComboBox)sender;
            SortingIndex = comboBox.SelectedIndex;
            Sort();
            scrollViewer?.ScrollToHome();
        }
    }

    private async void ColorsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (Instance != null && e.AddedItems is { Count: > 0 } && e.AddedItems[0] is ComboBoxItem)
        {
            var comboBox = (ComboBox)sender;
            ColorsNumberMode = (ColorsNumberMode)comboBox.SelectedIndex;
            Filtering.ColorsNumberMode = ColorsNumberMode;
            await UpdatePaletteList();

            scrollViewer?.ScrollToHome();
        }
    }

    private bool CanToggleFavourite(Palette palette)
    {
        return palette is { Colors.Count: > 0 };
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

        SortedResults?.Clear();

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
        if (Directory.Exists(Paths.PathToPalettesFolder))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Paths.PathToPalettesFolder,
                UseShellExecute = true,
                Verb = "open"
            });
        }
    }

    private async void AddFromCurrentPalette(object param)
    {
        if (CurrentEditingPalette?.Count == 0)
            return;

        string finalFileName = LocalPalettesFetcher.GetNonExistingName($"{new LocalizedString("UNNAMED_PALETTE").Value}.pal", true);
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
        string newPath = Path.Join(Paths.PathToPalettesFolder, finalNewName);

        if (newPath.Length > 250)
        {
            NoticeDialog.Show(stopItTexts[Random.Shared.Next(stopItTexts.Length - 1)], "NAME_IS_TOO_LONG");
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
        var parsers = PaletteProvider.AvailableParsers;
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
        // check if valid parser found
        var parser = await PaletteHelpers.GetValidParser(parsers, fileName);
        if (parser != null)
        {
            string name = LocalPalettesFetcher.GetNonExistingName(Path.GetFileName(fileName), true);
            await LocalPalettesFetcher.SavePalette(name, parser.Colors.ToArray());
        }
    }

    private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        ProcessHelpers.ShellExecute(e.Uri.ToString());
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);

        IPreferences.Current.RemoveCallback(PreferencesConstants.FavouritePalettes, OnFavouritePalettesChanged);
    }
}
