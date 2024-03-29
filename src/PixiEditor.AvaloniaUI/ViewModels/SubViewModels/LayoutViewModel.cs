using System.Collections.ObjectModel;
using PixiDocks.Core.Docking;
using PixiEditor.AvaloniaUI.ViewModels.Dock;

namespace PixiEditor.AvaloniaUI.ViewModels.SubViewModels;

internal class LayoutViewModel : SubViewModel<ViewModelMain>
{
    private LayoutManager layoutManagerManager;
    public LayoutManager LayoutManager
    {
        get => layoutManagerManager;
        private init => SetProperty(ref layoutManagerManager, value);
    }

    public LayoutViewModel(ViewModelMain owner) : base(owner)
    {
        LayoutManager = new();
        owner.WindowSubViewModel.ViewportAdded += WindowSubViewModel_ViewportAdded;
        owner.WindowSubViewModel.ViewportClosed += WindowSubViewModel_ViewportRemoved;
    }

    private void WindowSubViewModel_ViewportAdded(ViewportWindowViewModel obj)
    {
        LayoutManager.AddViewport(obj);
    }

    private void WindowSubViewModel_ViewportRemoved(ViewportWindowViewModel obj)
    {
        LayoutManager.RemoveViewport(obj);
    }
}
