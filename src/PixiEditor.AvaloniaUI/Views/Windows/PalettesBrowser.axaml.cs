using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Helpers.Extensions;
using PixiEditor.AvaloniaUI.Models.Dialogs;
using PixiEditor.AvaloniaUI.Models.ExtensionServices;
using PixiEditor.AvaloniaUI.Models.IO;
using PixiEditor.AvaloniaUI.Models.Palettes;
using PixiEditor.AvaloniaUI.Models.Structures;
using PixiEditor.AvaloniaUI.ViewModels.SubViewModels;
using PixiEditor.AvaloniaUI.Views.Dialogs;
using PixiEditor.AvaloniaUI.Views.Input;
using PixiEditor.AvaloniaUI.Views.Palettes;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.Common.UserPreferences;
using PixiEditor.Extensions.Palettes;
using PixiEditor.Extensions.Palettes.Parsers;
using PixiEditor.Extensions.Windowing;
using PixiEditor.Models.Enums;
using PixiEditor.OperatingSystem;
using PaletteColor = PixiEditor.Extensions.Palettes.PaletteColor;
using ViewModelMain = PixiEditor.AvaloniaUI.ViewModels.ViewModelMain;

namespace PixiEditor.AvaloniaUI.Views.Windows;

internal partial class PalettesBrowser : PixiEditorPopup, IPopupWindow
{
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

    public static readonly StyledProperty<PaletteList> PaletteListProperty =
        AvaloniaProperty.Register<PalettesBrowser, PaletteList>(nameof(PaletteList));

    public ICommand ImportPaletteCommand
    {
        get => (ICommand)GetValue(ImportPaletteCommandProperty);
        set => SetValue(ImportPaletteCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand> ImportPaletteCommandProperty =
        AvaloniaProperty.Register<PalettesBrowser, ICommand>(nameof(ImportPaletteCommand));

    public static readonly StyledProperty<ICommand> DeletePaletteCommandProperty =
        AvaloniaProperty.Register<PalettesBrowser, ICommand>(nameof(DeletePaletteCommand));

    public ICommand DeletePaletteCommand
    {
        get => (ICommand)GetValue(DeletePaletteCommandProperty);
        set => SetValue(DeletePaletteCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand> AddFromPaletteCommandProperty =
        AvaloniaProperty.Register<PalettesBrowser, ICommand>(nameof(AddFromPaletteCommand));

    public ICommand AddFromPaletteCommand
    {
        get { return (ICommand)GetValue(AddFromPaletteCommandProperty); }
        set { SetValue(AddFromPaletteCommandProperty, value); }
    }

    public bool IsFetching
    {
        get => GetValue(IsFetchingProperty);
        set => SetValue(IsFetchingProperty, value);
    }

    public static readonly StyledProperty<bool> IsFetchingProperty =
        AvaloniaProperty.Register<PalettesBrowser, bool>(nameof(IsFetching));

    public int ColorsNumber
    {
        get => (int)GetValue(ColorsNumberProperty);
        set => SetValue(ColorsNumberProperty, value);
    }

    public static readonly StyledProperty<int> ColorsNumberProperty =
        AvaloniaProperty.Register<PalettesBrowser, int>(nameof(ColorsNumber), 8);
    public bool SortAscending
    {
        get => (bool)GetValue(SortAscendingProperty);
        set => SetValue(SortAscendingProperty, value);
    }

    public static readonly StyledProperty<bool> SortAscendingProperty =
        AvaloniaProperty.Register<PalettesBrowser, bool>(nameof(SortAscending), true);


    public static readonly StyledProperty<ObservableRangeCollection<Palette>> SortedResultsProperty =
        AvaloniaProperty.Register<PalettesBrowser, ObservableRangeCollection<Palette>>(nameof(SortedResults));

    public ObservableRangeCollection<Palette> SortedResults
    {
        get => GetValue(SortedResultsProperty);
        set => SetValue(SortedResultsProperty, value);
    }

    public static readonly StyledProperty<string> NameFilterProperty =
        AvaloniaProperty.Register<PalettesBrowser, string>(nameof(NameFilter), string.Empty);

    public string NameFilter
    {
        get => (string)GetValue(NameFilterProperty);
        set => SetValue(NameFilterProperty, value);
    }

    public static readonly StyledProperty<bool> ShowOnlyFavouritesProperty =
        AvaloniaProperty.Register<PalettesBrowser, bool>(nameof(ShowOnlyFavourites), false);

    public bool ShowOnlyFavourites
    {
        get => (bool)GetValue(ShowOnlyFavouritesProperty);
        set => SetValue(ShowOnlyFavouritesProperty, value);
    }

    public static readonly StyledProperty<PaletteProvider> PaletteProviderProperty =
        AvaloniaProperty.Register<PalettesBrowser, PaletteProvider>(nameof(PaletteProvider));

    public PaletteProvider PaletteProvider
    {
        get { return GetValue(PaletteProviderProperty); }
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
    public ObservableRangeCollection<PaletteColor> CurrentEditingPalette { get; set; }
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

    static PalettesBrowser()
    {
        ColorsNumberProperty.Changed.Subscribe(ColorsNumberChanged);
        SortAscendingProperty.Changed.Subscribe(OnSortAscendingChanged);
        NameFilterProperty.Changed.Subscribe(OnNameFilterChanged);
        ShowOnlyFavouritesProperty.Changed.Subscribe(OnShowOnlyFavouritesChanged);
    }

    public PalettesBrowser(ColorsViewModel vm)
    {
        localizationProvider = ViewModelMain.Current.LocalizationProvider;
        localizationProvider.OnLanguageChanged += LocalizationProviderOnOnLanguageChanged;
        MinWidth = DetermineWidth();
        
        PaletteProvider = vm.PaletteProvider;
        ImportPaletteCommand = vm.ImportPaletteCommand;
        CurrentEditingPalette = vm.Owner.DocumentManagerSubViewModel.ActiveDocument?.Palette;
        InitializeComponent();
        Title = new LocalizedString("PALETTE_BROWSER");
        Instance = this;

        DeletePaletteCommand = new AsyncRelayCommand<Palette>(DeletePalette, CanDeletePalette);
        ToggleFavouriteCommand = new RelayCommand<Palette>(ToggleFavourite, CanToggleFavourite);
        AddFromPaletteCommand = new AsyncRelayCommand(AddFromCurrentPalette, CanAddFromPalette);
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

    public async Task<bool?> ShowDialog()
    {
        bool result = false;
        await Application.Current.ForDesktopMainWindowAsync(async (window) =>
        {
            result = await ShowDialog<bool>(window);
        });

        return result;
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

    private bool CanAddFromPalette()
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

    public static PalettesBrowser Open()
    {
        if (Instance != null) return Instance;

        Window owner = null;
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            owner = desktop.MainWindow;
        }

        PalettesBrowser browser = new PalettesBrowser(ViewModelMain.Current.ColorsSubViewModel)
        {
            Owner = owner,
        };

        browser.Show();
        return browser;
    }

    private async void LocalCacheRefreshed(RefreshType refreshType, Palette itemAffected, string fileNameAffected)
    {
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            SortedResults ??= new ObservableRangeCollection<Palette>();
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
        if (!SortedResults.Contains(updatedItem))
        {
            SortedResults.Add(updatedItem);
        }

        if(!PaletteList.Palettes.Contains(updatedItem))
        {
            PaletteList.Palettes.Add(updatedItem);
        }

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

    private async Task DeletePalette(Palette palette)
    {
        if (palette == null) return;

        string filePath = Path.Join(Paths.PathToPalettesFolder, palette.FileName);
        if (File.Exists(filePath))
        {
            if (await ConfirmationDialog.Show("DELETE_PALETTE_CONFIRMATION", "WARNING") == ConfirmationType.Yes)
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

    private static async void OnShowOnlyFavouritesChanged(AvaloniaPropertyChangedEventArgs<bool> e)
    {
        PalettesBrowser browser = (PalettesBrowser)e.Sender;
        browser.Filtering.ShowOnlyFavourites = e.NewValue.Value;
        await browser.UpdatePaletteList();
    }

    private static async void ColorsNumberChanged(AvaloniaPropertyChangedEventArgs<int> e)
    {
        PalettesBrowser browser = (PalettesBrowser)e.Sender;
        browser.Filtering.ColorsCount = e.NewValue.Value;
        await browser.UpdatePaletteList();
    }

    private static void OnSortAscendingChanged(AvaloniaPropertyChangedEventArgs<bool> d)
    {
        PalettesBrowser browser = d.Sender as PalettesBrowser;
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

    private void AddToPaletteList(ObservableRangeCollection<Palette> srcPalettes)
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
        ObservableRangeCollection<Palette> palettes = new ObservableRangeCollection<Palette>();
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

    private static async void OnNameFilterChanged(AvaloniaPropertyChangedEventArgs<string> e)
    {
        var browser = (PalettesBrowser)e.Sender;
        browser.Filtering.Name = browser.NameFilter;
        await browser.UpdatePaletteList();
        browser.scrollViewer.ScrollToHome();
    }

    private async void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (PaletteList?.Palettes == null) return;
        var viewer = (ScrollViewer)sender;
        if (viewer.Offset.Y == viewer.Viewport.Height && _lastScrolledOffset != viewer.Offset.Y)
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

            _lastScrolledOffset = viewer.Offset.Y;
        }
    }

    private bool PaletteEquals(Palette palette, ObservableRangeCollection<Palette> paletteListPalettes)
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

    private void Grid_MouseDown(object? sender, PointerPressedEventArgs pointerPressedEventArgs)
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
                case SortingType.Default:
                    sorted = PaletteList.Palettes.OrderByDescending(x => x.IsFavourite).ThenBy(x => PaletteList.Palettes.IndexOf(x));
                    break;
                case SortingType.Alphabetical:
                    sorted = PaletteList.Palettes.OrderBy(x => x.Name);
                    break;
                case SortingType.ColorCount:
                    sorted = PaletteList.Palettes.OrderBy(x => x.Colors.Count);
                    break;
            }
        }
        else
        {
            switch (InternalSortingType)
            {
                case SortingType.Default:
                    sorted = PaletteList.Palettes.OrderByDescending(x => PaletteList.Palettes.IndexOf(x));
                    break;
                case SortingType.Alphabetical:
                    sorted = PaletteList.Palettes.OrderByDescending(x => x.Name);
                    break;
                case SortingType.ColorCount:
                    sorted = PaletteList.Palettes.OrderByDescending(x => x.Colors.Count);
                    break;
            }
        }

        if (sorted != null)
        {
            SortedResults = new ObservableRangeCollection<Palette>(sorted);
        }
    }

    private void OpenFolder_OnClick(object sender, RoutedEventArgs e)
    {
        if (Directory.Exists(Paths.PathToPalettesFolder))
        {
            IOperatingSystem.Current.OpenFolder(Paths.PathToPalettesFolder);
        }
    }

    private async Task AddFromCurrentPalette()
    {
        if (CurrentEditingPalette?.Count == 0)
            return;

        string finalFileName = LocalPalettesFetcher.GetNonExistingName($"{new LocalizedString("UNNAMED_PALETTE").Value}.pal", true);
        await LocalPalettesFetcher.SavePalette(finalFileName, CurrentEditingPalette.ToArray());
    }

    private void PaletteItem_OnRename(object? sender, EditableTextBlock.TextChangedEventArgs e)
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

        IOperatingSystem.Current.OpenUri(url);
    }


    private async void ImportFromFile_OnClick(object? sender, RoutedEventArgs e)
    {
        var parsers = PaletteProvider.AvailableParsers;
        var file = await TopLevel.GetTopLevel(this).StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions()
            {
                FileTypeFilter = PaletteHelpers.GetFilter(parsers, true), AllowMultiple = false
            });

        if (file is { Count: > 0 })
        {
            var fileName = file[0].Path.AbsolutePath;
            await ImportPalette(fileName, parsers);
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

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        IPreferences.Current.RemoveCallback(PreferencesConstants.FavouritePalettes, OnFavouritePalettesChanged);
    }
}
