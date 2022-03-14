using PixiEditor.Models.DataHolders;
using PixiEditor.Models.DataHolders.Palettes;
using PixiEditor.Models.DataProviders;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Events;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            "NameFilter", typeof(string), typeof(PalettesBrowser), new PropertyMetadata(default(string)));

        public string NameFilter
        {
            get { return (string)GetValue(NameFilterProperty); }
            set { SetValue(NameFilterProperty, value); }
        }

        public string SortingType { get; set; } = "Default";
        public ColorsNumberMode ColorsNumberMode { get; set; } = ColorsNumberMode.Any;
        public string[] Tags { get; set; } = Array.Empty<string>();

        private FilteringSettings _filteringSettings;

        public FilteringSettings Filtering => _filteringSettings ??= new FilteringSettings(ColorsNumberMode, ColorsNumber, Tags, NameFilter);

        private char[] _separators = new char[] { ' ', ',' };

        private SortingType _sortingType => (SortingType)Enum.Parse(typeof(SortingType), SortingType.Replace(" ", ""));

        public PalettesBrowser()
        {
            InitializeComponent();
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
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

        private async void TagsInput_OnSubmit(object sender, InputBoxEventArgs e)
        {
            Tags = e.Input.Split(_separators, options: StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            Filtering.Tags = Tags;
            await UpdatePaletteList();
            scrollViewer.ScrollToHome();
        }

        private async void NameInput_OnSubmit(object sender, InputBoxEventArgs e)
        {
            NameFilter = e.Input;
            Filtering.Name = NameFilter;
            await UpdatePaletteList();
            scrollViewer.ScrollToHome();
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
                        sorted = PaletteList.Palettes.OrderBy(x => PaletteList.Palettes.IndexOf(x));
                        break;
                    case Models.DataHolders.Palettes.SortingType.Alphabetical:
                        sorted = PaletteList.Palettes.OrderBy(x => x.Title);
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
                        sorted = PaletteList.Palettes.OrderByDescending(x => x.Title);
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
    }
}
