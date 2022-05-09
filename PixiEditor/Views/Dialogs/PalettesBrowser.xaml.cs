using PixiEditor.Models.DataHolders;
using PixiEditor.Models.DataHolders.Palettes;
using PixiEditor.Models.DataProviders;
using PixiEditor.Models.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using PixiEditor.Views.UserControls.Palettes;
using SkiaSharp;
using PixiEditor.Helpers;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.UserPreferences;

namespace PixiEditor.Views.Dialogs
{
    public delegate void ListFetched(PaletteList list);

    /// <summary>
    /// Interaction logic for LospecPalettesBrowser.xaml
    /// </summary>
    public partial class PalettesBrowser : Window
    {
        public event ListFetched OnListFetched;
        public const int ItemsPerLoad = 10;

        public PaletteList PaletteList
        {
            get { return (PaletteList)GetValue(PaletteListProperty); }
            set { SetValue(PaletteListProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PaletteList.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PaletteListProperty =
            DependencyProperty.Register("PaletteList", typeof(PaletteList), typeof(PalettesBrowser));

        public ICommand ImportPaletteCommand
        {
            get { return (ICommand)GetValue(ImportPaletteCommandProperty); }
            set { SetValue(ImportPaletteCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImportPaletteCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImportPaletteCommandProperty =
            DependencyProperty.Register("ImportPaletteCommand", typeof(ICommand), typeof(PalettesBrowser));

        public static readonly DependencyProperty DeletePaletteCommandProperty = DependencyProperty.Register(
            "DeletePaletteCommand", typeof(ICommand), typeof(PalettesBrowser), new PropertyMetadata(default(ICommand)));

        public ICommand DeletePaletteCommand
        {
            get { return (ICommand)GetValue(DeletePaletteCommandProperty); }
            set { SetValue(DeletePaletteCommandProperty, value); }
        }

        public bool IsFetching
        {
            get { return (bool)GetValue(IsFetchingProperty); }
            set { SetValue(IsFetchingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsFetching.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFetchingProperty =
            DependencyProperty.Register("IsFetching", typeof(bool), typeof(PalettesBrowser), new PropertyMetadata(false));

        public int ColorsNumber
        {
            get { return (int)GetValue(ColorsNumberProperty); }
            set { SetValue(ColorsNumberProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ColorsNumber.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorsNumberProperty =
            DependencyProperty.Register("ColorsNumber", typeof(int), typeof(PalettesBrowser),
                new PropertyMetadata(8, ColorsNumberChanged));

        public WpfObservableRangeCollection<PaletteListDataSource> PaletteListDataSources
        {
            get { return (WpfObservableRangeCollection<PaletteListDataSource>)GetValue(PaletteListDataSourcesProperty); }
            set { SetValue(PaletteListDataSourcesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PaletteListDataSources.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PaletteListDataSourcesProperty =
            DependencyProperty.Register("PaletteListDataSources", typeof(WpfObservableRangeCollection<PaletteListDataSource>), typeof(PalettesBrowser), new PropertyMetadata(new WpfObservableRangeCollection<PaletteListDataSource>()));
        public bool SortAscending
        {
            get { return (bool)GetValue(SortAscendingProperty); }
            set { SetValue(SortAscendingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SortDescending.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SortAscendingProperty =
            DependencyProperty.Register("SortAscending", typeof(bool), typeof(PalettesBrowser), new PropertyMetadata(true, OnSortAscendingChanged));


        public static readonly DependencyProperty SortedResultsProperty = DependencyProperty.Register(
            "SortedResults", typeof(WpfObservableRangeCollection<Palette>), typeof(PalettesBrowser), new PropertyMetadata(default(WpfObservableRangeCollection<Palette>)));

        public WpfObservableRangeCollection<Palette> SortedResults
        {
            get { return (WpfObservableRangeCollection<Palette>)GetValue(SortedResultsProperty); }
            set { SetValue(SortedResultsProperty, value); }
        }

        public static readonly DependencyProperty NameFilterProperty = DependencyProperty.Register(
            "NameFilter", typeof(string), typeof(PalettesBrowser),
            new PropertyMetadata(default(string), OnNameFilterChanged));

        public string NameFilter
        {
            get { return (string)GetValue(NameFilterProperty); }
            set { SetValue(NameFilterProperty, value); }
        }

        public static readonly DependencyProperty ShowOnlyFavouritesProperty = DependencyProperty.Register(
            "ShowOnlyFavourites", typeof(bool), typeof(PalettesBrowser),
            new PropertyMetadata(false, OnShowOnlyFavouritesChanged));

        public bool ShowOnlyFavourites
        {
            get { return (bool)GetValue(ShowOnlyFavouritesProperty); }
            set { SetValue(ShowOnlyFavouritesProperty, value); }
        }

        public RelayCommand<Palette> ToggleFavouriteCommand { get; set; }

        public string SortingType { get; set; } = "Default";
        public ColorsNumberMode ColorsNumberMode { get; set; } = ColorsNumberMode.Any;

        private FilteringSettings _filteringSettings;

        public FilteringSettings Filtering => _filteringSettings ??=
            new FilteringSettings(ColorsNumberMode, ColorsNumber, NameFilter, ShowOnlyFavourites);

        private char[] _separators = new char[] { ' ', ',' };

        private SortingType _sortingType => (SortingType)Enum.Parse(typeof(SortingType), SortingType.Replace(" ", ""));
        public WpfObservableRangeCollection<SKColor> CurrentEditingPalette { get; set; }
        public static PalettesBrowser Instance { get; internal set; }

        private LocalPalettesFetcher localPalettesFetcher
        {
            get
            {
                return _localPalettesFetcher ??= (LocalPalettesFetcher)PaletteListDataSources.First(x => x is LocalPalettesFetcher);
            }
        }

        private static PaletteList _cachedPaletteList;

        private LocalPalettesFetcher _localPalettesFetcher;

        public PalettesBrowser()
        {
            InitializeComponent();
            Instance = this;
            DeletePaletteCommand = new RelayCommand<Palette>(DeletePalette);
            ToggleFavouriteCommand = new RelayCommand<Palette>(ToggleFavourite);
            Loaded += (sender, args) =>
            {
                localPalettesFetcher.CacheUpdated += LocalCacheRefreshed;
            };
            Closed += (s, e) =>
            {
                Instance = null;
                localPalettesFetcher.CacheUpdated -= LocalCacheRefreshed;
            };
        }

        public static PalettesBrowser Open(WpfObservableRangeCollection<PaletteListDataSource> dataSources, ICommand importPaletteCommand, WpfObservableRangeCollection<SKColor> currentEditingPalette)
        {
            if (Instance != null) return Instance;
            PalettesBrowser browser = new PalettesBrowser
            {
                Owner = Application.Current.MainWindow,
                ImportPaletteCommand = importPaletteCommand,
                PaletteListDataSources = dataSources
            };

            if (_cachedPaletteList != null)
            {
                browser.PaletteList = _cachedPaletteList;
            }

            browser.OnListFetched += list =>
            {
                _cachedPaletteList = list;
            };

            browser.CurrentEditingPalette = currentEditingPalette;

            browser.Show();
            return browser;
        }

        private async void LocalCacheRefreshed(List<Palette> obj)
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                await UpdatePaletteList();
            });
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

        private async void DeletePalette(Palette palette)
        {
            if (palette == null) return;

            string filePath = Path.Join(LocalPalettesFetcher.PathToPalettesFolder, palette.FileName);
            if (File.Exists(filePath))
            {
                if (ConfirmationDialog.Show("Are you sure you want to delete this palette? This cannot be undone.", "Warning!") == ConfirmationType.Yes)
                {
                    LocalPalettesFetcher.DeletePalette(palette.FileName);
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

        private async static void OnShowOnlyFavouritesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
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
            OnListFetched?.Invoke(PaletteList);

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
            var scrollViewer = (ScrollViewer)sender;
            if (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
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
                OnListFetched?.Invoke(PaletteList);
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
            if (e.AddedItems is { Count: > 0 } && e.AddedItems[0] is ComboBoxItem item && item.Content is string value)
            {
                ColorsNumberMode = Enum.Parse<ColorsNumberMode>(value);
                Filtering.ColorsNumberMode = ColorsNumberMode;
                await UpdatePaletteList();
                scrollViewer.ScrollToHome();
            }
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
            if(PaletteList?.Palettes == null) return;

            IOrderedEnumerable<Palette> sorted = null;
            if (!descending)
            {
                switch (_sortingType)
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
                switch (_sortingType)
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

            if(sorted != null)
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
            if (CurrentEditingPalette?.Count == 0) return;

            string finalFileName = "Unnamed Palette.pal";

            string path = Path.Join(LocalPalettesFetcher.PathToPalettesFolder, finalFileName);
            int i = 1;
            while (File.Exists(path))
            {
                finalFileName = $"Unnamed Palette {i}.pal";
                path = Path.Join(LocalPalettesFetcher.PathToPalettesFolder, finalFileName);
                i++;
            }

            await LocalPalettesFetcher.SavePalette(finalFileName, CurrentEditingPalette.ToArray());

            var palette = _localPalettesFetcher.CachedPalettes.FirstOrDefault(x => x.FileName == finalFileName);
            if (palette != null)
            {
                if (SortedResults.Contains(palette))
                {
                    SortedResults.Move(SortedResults.IndexOf(palette), 0);
                }
                else
                {
                    SortedResults.Insert(0, palette);
                }
            }
        }

        private async void PaletteItem_OnRename(object sender, EditableTextBlock.TextChangedEventArgs e)
        {
            PaletteItem item = (PaletteItem)sender;

            string oldFileName = $"{e.OldText}.pal";

            if (string.IsNullOrWhiteSpace(e.NewText) || e.NewText == item.Palette.Name)
            {
                item.Palette.FileName = oldFileName;
                item.Palette.Name = e.OldText;
                return;
            }

            string oldPath = Path.Join(LocalPalettesFetcher.PathToPalettesFolder, oldFileName);

            if (!File.Exists(oldPath))
            {
                item.Palette.FileName = oldFileName;
                item.Palette.Name = e.OldText;
                return;
            }

            string finalNewName = $"{e.NewText}.pal";
            string newPath = Path.Join(LocalPalettesFetcher.PathToPalettesFolder, LocalPalettesFetcher.GetNonExistingName(finalNewName, true));

            File.Move(oldPath, newPath);

            item.Palette.FileName = finalNewName;
        }

        private void BrowseOnLospec_OnClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string url = (string)button.CommandParameter;

            ProcessHelpers.ShellExecute(url);
        }
    }
}
