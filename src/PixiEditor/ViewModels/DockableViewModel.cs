using Avalonia.Media;
using PixiDocks.Core.Docking;
using PixiDocks.Core.Docking.Events;
using PixiEditor.ViewModels.Dock;

namespace PixiEditor.ViewModels;

internal abstract class DockableViewModel : ViewModelBase, IDockableContent
{
    public abstract string Id { get; }
    public abstract string Title { get; }
    public abstract bool CanFloat { get; }
    public abstract bool CanClose { get; }
    public TabCustomizationSettings TabCustomizationSettings { get; protected set; } = new();

    public DockableViewModel()
    {
    }
}
