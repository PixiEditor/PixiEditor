using Avalonia.Media;
using PixiDocks.Core.Docking;
using PixiDocks.Core.Docking.Events;
using PixiEditor.AvaloniaUI.ViewModels.Dock;

namespace PixiEditor.AvaloniaUI.ViewModels;

internal abstract class DockableViewModel : ViewModelBase, IDockableContent
{
    public abstract string Id { get; }
    public abstract string Title { get; }
    public abstract bool CanFloat { get; }
    public abstract bool CanClose { get; }
    public abstract IImage? Icon { get; }

    object? IDockableContent.Icon => Icon;

    public DockableViewModel()
    {
    }
}
