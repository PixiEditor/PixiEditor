using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Events;
using PixiEditor.Models.ExternalServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PixiEditor.Views.Dialogs
{
    public delegate void ListFetched(PaletteList list);

    /// <summary>
    /// Interaction logic for LospecPalettesBrowser.xaml
    /// </summary>
    public partial class LospecPalettesBrowser : Window
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
            DependencyProperty.Register("PaletteList", typeof(PaletteList), typeof(LospecPalettesBrowser));

        public ICommand ImportPaletteCommand
        {
            get { return (ICommand)GetValue(ImportPaletteCommandProperty); }
            set { SetValue(ImportPaletteCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImportPaletteCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImportPaletteCommandProperty =
            DependencyProperty.Register("ImportPaletteCommand", typeof(ICommand), typeof(LospecPalettesBrowser));

        public bool IsFetching
        {
            get { return (bool)GetValue(IsFetchingProperty); }
            set { SetValue(IsFetchingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsFetching.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFetchingProperty =
            DependencyProperty.Register("IsFetching", typeof(bool), typeof(LospecPalettesBrowser), new PropertyMetadata(false));

        public string SortingType { get; set; } = "Default";
        public string[] Tags { get; set; } = Array.Empty<string>();

        private char[] _separators = new char[] { ' ', ',' };


        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }


        public LospecPalettesBrowser()
        {
            InitializeComponent();
        }

        public async Task UpdatePaletteList(bool refetch = false)
        {
            IsFetching = true;
            if (PaletteList == null || refetch)
            {
                PaletteList = await LospecPaletteFetcher.FetchPage(0, SortingType.ToLower(), Tags);
                OnListFetched.Invoke(PaletteList);
            }

            IsFetching = false;
        }

        private async void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (PaletteList == null || PaletteList.Palettes == null) return;
            var scrollViewer = (ScrollViewer)sender;
            if (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
            {
                IsFetching = true;
                _currentPage++;
                var newPalettes = await LospecPaletteFetcher.FetchPage(_currentPage);
                if(newPalettes == null || !newPalettes.FetchedCorrectly || newPalettes.Palettes == null)
                {
                    IsFetching = false;
                    return;
                }

                PaletteList.Palettes.AddRange(newPalettes.Palettes);
                OnListFetched.Invoke(PaletteList);
                IsFetching = false;
            }
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

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ((Grid)sender).Focus();
        }
    }
}
