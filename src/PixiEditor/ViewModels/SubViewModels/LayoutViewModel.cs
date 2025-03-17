using System.Collections.ObjectModel;
using Avalonia.Input;
using PixiDocks.Core.Docking;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.ViewModels.Dock;

namespace PixiEditor.ViewModels.SubViewModels;

internal class LayoutViewModel : SubViewModel<ViewModelMain>
{
    private LayoutManager layoutManagerManager;
    public LayoutManager LayoutManager
    {
        get => layoutManagerManager;
        private init => SetProperty(ref layoutManagerManager, value);
    }

    public LayoutViewModel(ViewModelMain owner, LayoutManager layoutManager) : base(owner)
    {
        LayoutManager = layoutManager;
        owner.WindowSubViewModel.ViewportAdded += WindowSubViewModel_ViewportAdded;
        owner.WindowSubViewModel.LazyViewportAdded += WindowSubViewModel_LazyViewportAdded;
        owner.WindowSubViewModel.ViewportClosed += WindowSubViewModel_ViewportRemoved;
        owner.WindowSubViewModel.LazyViewportRemoved += WindowSubViewModel_LazyViewportRemoved;
    }

    private void WindowSubViewModel_ViewportAdded(ViewportWindowViewModel obj)
    {
        LayoutManager.AddViewport(obj);
    }

    private void WindowSubViewModel_ViewportRemoved(ViewportWindowViewModel obj)
    {
        LayoutManager.RemoveViewport(obj);
    }

    private void WindowSubViewModel_LazyViewportAdded(LazyViewportWindowViewModel obj)
    {
        LayoutManager.AddViewport(obj);
    }

    private void WindowSubViewModel_LazyViewportRemoved(LazyViewportWindowViewModel obj)
    {
        LayoutManager.RemoveViewport(obj);
    }
}
