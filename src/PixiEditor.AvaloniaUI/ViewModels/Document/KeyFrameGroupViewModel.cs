using System.Collections.ObjectModel;
using PixiEditor.AvaloniaUI.Models.Handlers;

namespace PixiEditor.AvaloniaUI.ViewModels.Document;

public class KeyFrameGroupViewModel : KeyFrameViewModel, IKeyFrameGroupHandler
{
    public ObservableCollection<IKeyFrameHandler> Children { get; } = new ObservableCollection<IKeyFrameHandler>();

    public override int StartFrame => Children.Count > 0 ? Children.Min(x => x.StartFrame) : 0;
    public override int Duration => Children.Count > 0 ? Children.Max(x => x.StartFrame + x.Duration) : 0;

    public KeyFrameGroupViewModel(int startFrame, int duration, Guid layerGuid, Guid id) : base(startFrame, duration, layerGuid, id)
    {
    }
}
