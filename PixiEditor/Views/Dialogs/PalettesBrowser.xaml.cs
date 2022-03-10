using PixiEditor.Models.DataHolders;
using PixiEditor.Models.DataHolders.Palettes;
using PixiEditor.Models.DataProviders;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Events;
using System;
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

        public WpfObservableRangeCollection<Palette> FilteredPalettes
        {
            get { return (WpfObservableRangeCollection<Palette>)GetValue(FilteredPalettesProperty); }
            set { SetValue(FilteredPalettesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FilteredPalettes.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilteredPalettesProperty =
            DependencyProperty.Register("FilteredPalettes", typeof(WpfObservableRangeCollection<Palette>), typeof(PalettesBrowser));




        public string SortingType { get; set; } = "Default";
        public ColorsNumberMode ColorsNumberMode { get; set; } = ColorsNumberMode.Any;
        public string[] Tags { get; set; } = Array.Empty<string>();

        private char[] _separators = new char[] { ' ', ',' };

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

        private static void ColorsNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PalettesBrowser browser = (PalettesBrowser)d;
            browser.FilterPalettes();
        }


        public async Task UpdatePaletteList(bool refetch = false)
        {
            IsFetching = true;
            if (PaletteList == null || refetch)
            {
                for (int i = 0; i < PaletteListDataSources.Count; i++)
                {
                    PaletteList src = await FetchPaletteList(i);
                    if (!src.FetchedCorrectly) continue;
                    if (PaletteList == null)
                    {
                        PaletteList = src;
                    }
                    else
                    {
                        PaletteList.Palettes.AddRange(src.Palettes);
                    }
                }

                if (FilteredPalettes == null)
                {
                    FilteredPalettes = new WpfObservableRangeCollection<Palette>(PaletteList.Palettes);
                }

                FilterPalettes();
                OnListFetched.Invoke(PaletteList);
            }

            IsFetching = false;
        }

        private async Task<PaletteList> FetchPaletteList(int index)
        {
            int startIndex = PaletteList != null ? PaletteList.Palettes.Count : 0;
            var src = await PaletteListDataSources[index].FetchPaletteList(startIndex, ItemsPerLoad);
            return src;
        }

        private async void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (PaletteList == null || PaletteList.Palettes == null) return;
            var scrollViewer = (ScrollViewer)sender;
            if (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
            {
                IsFetching = true;
                var newPalettes = await FetchPaletteList(0);
                if (newPalettes == null || !newPalettes.FetchedCorrectly || newPalettes.Palettes == null)
                {
                    IsFetching = false;
                    return;
                }

                PaletteList.Palettes.AddRange(newPalettes.Palettes);
                FilterPalettes();
                OnListFetched.Invoke(PaletteList);
                IsFetching = false;
            }
        }

        private void SortingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count > 0 && e.AddedItems[0] is ComboBoxItem item && item.Content is string value)
            {
                SortingType = value;
                FilterPalettes();
                scrollViewer.ScrollToHome();
            }
        }

        private void FilterPalettes()
        {
            FilteredPalettes.Clear();
            switch (ColorsNumberMode)
            {
                case ColorsNumberMode.Any:
                    FilteredPalettes.AddRange(PaletteList.Palettes);
                    break;
                case ColorsNumberMode.Max:
                    FilteredPalettes.AddRange(PaletteList.Palettes.Where(x => x.Colors.Count <= ColorsNumber));
                    break;
                case ColorsNumberMode.Min:
                    FilteredPalettes.AddRange(PaletteList.Palettes.Where(x => x.Colors.Count >= ColorsNumber));
                    break;
                case ColorsNumberMode.Exact:
                    FilteredPalettes.AddRange(PaletteList.Palettes.Where(x => x.Colors.Count == ColorsNumber));
                    break;
                default:
                    break;
            }

            
            FilteredPalettes.OrderBy()
        }

        private void TagsInput_OnSubmit(object sender, InputBoxEventArgs e)
        {
            Tags = e.Input.Split(_separators, options: StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            FilterPalettes();
            scrollViewer.ScrollToHome();
        }


        private void ColorsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count > 0 && e.AddedItems[0] is ComboBoxItem item && item.Content is string value)
            {
                ColorsNumberMode = Enum.Parse<ColorsNumberMode>(value);
                FilterPalettes();
                scrollViewer.ScrollToHome();
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ((Grid)sender).Focus();
        }
    }
}
