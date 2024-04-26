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

    public LayoutViewModel(ViewModelMain owner, WindowViewModel windowViewModel) : base(owner)
    {
        LayoutManager = new();
        windowViewModel.ViewportAdded += WindowSubViewModel_ViewportAdded;
        windowViewModel.ViewportClosed += WindowSubViewModel_ViewportRemoved;
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
