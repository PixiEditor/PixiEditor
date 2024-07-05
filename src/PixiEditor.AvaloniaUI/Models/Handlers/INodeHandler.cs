using System.Collections.ObjectModel;
using ChunkyImageLib;
using PixiEditor.Numerics;

namespace PixiEditor.AvaloniaUI.Models.Handlers;

public interface INodeHandler
{
    public Guid Id { get; }
    public string NodeName { get; set; }
    public ObservableCollection<IInputPropertyHandler> Inputs { get; }
    public ObservableCollection<IOutputPropertyHandler> Outputs { get; }
    public Surface ResultPreview { get; set; }
    public VecD Position { get; set; }
    void TraverseBackwards(Func<INodeHandler, bool> func);
    void TraverseForwards(Func<INodeHandler, bool> func);
}
