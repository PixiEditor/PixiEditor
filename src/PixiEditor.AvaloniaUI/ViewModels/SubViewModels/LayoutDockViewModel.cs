using Dock.Model.Controls;
using PixiEditor.AvaloniaUI.ViewModels.Dock;

namespace PixiEditor.AvaloniaUI.ViewModels.SubViewModels;

internal class LayoutDockViewModel : SubViewModel<ViewModelMain>
{
    private IRootDock layout;

    public IRootDock Layout
    {
        get => layout;
        set => SetProperty(ref layout, value);
    }

    public LayoutDockViewModel(ViewModelMain owner) : base(owner)
    {
        DockFactory factory = new(owner.FileSubViewModel);
        Layout = factory.CreateLayout();
        if (Layout is { })
        {
            factory.InitLayout(Layout);
        }
    }
}
