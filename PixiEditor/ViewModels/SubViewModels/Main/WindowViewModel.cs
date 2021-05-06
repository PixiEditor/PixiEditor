using AvalonDock.Layout;
using PixiEditor.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class WindowViewModel : SubViewModel<ViewModelMain>, ISetableOwner<ViewModelMain>
    {
        public MainWindow MainWindow { get; private set; }

        public RelayCommand ShowAvalonDockWindowCommand { get; set; }

        public WindowViewModel()
            : this(null)
        {
        }

        public WindowViewModel(ViewModelMain owner)
            : base(owner)
        {
            ShowAvalonDockWindowCommand = new RelayCommand(ShowAvalonDockWindow);

            MainWindow = (MainWindow)System.Windows.Application.Current?.MainWindow;
        }

        public void SetOwner(ViewModelMain owner)
        {
            Owner = owner;
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
