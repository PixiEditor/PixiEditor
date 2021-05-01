using AvalonDock.Layout;
using PixiEditor.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class WindowViewModel : SubViewModel<ViewModelMain>
    {
        public MainWindow MainWindow { get; private set; }

        public RelayCommand ShowAvalonDockWindowCommand { get; set; }

        public WindowViewModel(ViewModelMain owner)
            : base(owner)
        {
            ShowAvalonDockWindowCommand = new RelayCommand(ShowAvalonDockWindow);

            MainWindow = (MainWindow)System.Windows.Application.Current?.MainWindow;
        }

        private void ShowAvalonDockWindow(object parameter)
        {
            string id = (string)parameter;

            var anchorables = new List<LayoutAnchorable>(MainWindow.LayoutRoot.Manager.Layout
                    .Descendents()
                    .OfType<LayoutAnchorable>());

            foreach (var la in anchorables)
            {
                if (la.ContentId == id)
                {
                    la.Show();
                    la.IsActive = true;
                }
            }
        }
    }
}
