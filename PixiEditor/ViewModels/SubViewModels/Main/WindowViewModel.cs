using AvalonDock.Layout;
using GalaSoft.MvvmLight.CommandWpf;
using System.Collections.Generic;
using System.Linq;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class WindowViewModel : SubViewModel<ViewModelMain>, ISettableOwner<ViewModelMain>
    {
        public RelayCommand<string> ShowAvalonDockWindowCommand { get; set; }

        public WindowViewModel()
            : this(null)
        {
        }

        public WindowViewModel(ViewModelMain owner)
            : base(owner)
        {
            ShowAvalonDockWindowCommand = new(ShowAvalonDockWindow);
        }

        public void SetOwner(ViewModelMain owner)
        {
            Owner = owner;
        }

        private void ShowAvalonDockWindow(string id)
        {
            var anchorables = new List<LayoutAnchorable>(MainWindow.Current.LayoutRoot.Manager.Layout
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
