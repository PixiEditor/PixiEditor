using Drawie.Backend.Core.ColorsImpl;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface IQuickColorChangeable
{
    public IChangeInfo[] ChangeColor(int frame, params Color?[] colors);
    public Color?[] GetColors(int frame);
}
