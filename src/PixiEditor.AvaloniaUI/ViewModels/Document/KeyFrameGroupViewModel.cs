using System.Collections.ObjectModel;
using PixiEditor.AvaloniaUI.Models.Handlers;

namespace PixiEditor.AvaloniaUI.ViewModels.Document;

public class KeyFrameGroupViewModel : KeyFrameViewModel
{
    public ObservableCollection<KeyFrameViewModel> Children { get; } = new ObservableCollection<KeyFrameViewModel>();

    public override int StartFrame => Children.Min(x => x.StartFrame);
    public override int Duration => Children.Max(x => x.StartFrame + x.Duration);

    public KeyFrameGroupViewModel(int startFrame, int duration, Guid layerGuid, Guid id) : base(startFrame, duration, layerGuid, id)
    {
    }
}
