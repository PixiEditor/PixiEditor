using PixiEditor.Models.Commands.Search;
using PixiEditor.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace PixiEditor.Views.UserControls
{
    /// <summary>
    /// Interaction logic for CommandSearchControl.xaml
    /// </summary>
    public partial class CommandSearchControl : UserControl
    {
        public CommandSearchControl()
        {
            InitializeComponent();
        }

        private void Button_MouseMove(object sender, MouseEventArgs e)
        {
            var dataContext = mainGrid.DataContext as CommandSearchViewModel;
            var searchResult = (sender as Button).DataContext as SearchResult;

            dataContext.SelectedResult = searchResult;
        }
    }
}
