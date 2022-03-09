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
        private int _currentPage = 0;

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

        private static async void ColorsNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PalettesBrowser browser = (PalettesBrowser)d;
            await browser.UpdatePaletteList(true);
        }


        public async Task UpdatePaletteList(bool refetch = false)
        {
            IsFetching = true;
            if (PaletteList == null || refetch)
            {
                //PaletteList.Palettes.Clear();
                for (int i = 0; i < PaletteListDataSources.Count; i++)
                {
                    var src = await PaletteListDataSources[i].FetchPaletteList();
                    if (!src.FetchedCorrectly) continue;
                    //PaletteList.Palettes.AddRange(src.Palettes); //WHY TF IT DOESN'T UPDATE IN VIEW
                    PaletteList = src; // So yeah, since we don't have more than 1 palette data source right now, then temp solution
                }

                OnListFetched.Invoke(PaletteList);
            }

            IsFetching = false;
        }

        private async void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            //if (PaletteList == null || PaletteList.Palettes == null) return;
            //var scrollViewer = (ScrollViewer)sender;
            //if (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
            //{
            //    IsFetching = true;
            //    _currentPage++;
            //    var newPalettes = await LospecPaletteFetcher.FetchPage(_currentPage);
            //    if(newPalettes == null || !newPalettes.FetchedCorrectly || newPalettes.Palettes == null)
            //    {
            //        IsFetching = false;
            //        return;
            //    }

            //    PaletteList.Palettes.AddRange(newPalettes.Palettes);
            //    OnListFetched.Invoke(PaletteList);
            //    IsFetching = false;
            //}
        }

        private async void SortingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count > 0 && e.AddedItems[0] is ComboBoxItem item && item.Content is string value)
            {
                SortingType = value;
                await UpdatePaletteList(true);
                scrollViewer.ScrollToHome();
            }
        }

        private async void TagsInput_OnSubmit(object sender, InputBoxEventArgs e)
        {
            Tags = e.Input.Split(_separators, options: StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            await UpdatePaletteList(true);
            scrollViewer.ScrollToHome();
        }


        private async void ColorsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count > 0 && e.AddedItems[0] is ComboBoxItem item && item.Content is string value)
            {
                ColorsNumberMode = Enum.Parse<ColorsNumberMode>(value);
                await UpdatePaletteList(true);
                scrollViewer.ScrollToHome();
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ((Grid)sender).Focus();
        }
    }
}
