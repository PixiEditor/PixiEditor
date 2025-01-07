using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Animations;

namespace PixiEditor.ChangeableDocument.Rendering;

public struct PreviewRequest
{
    public VecI Size { get; }
    public KeyFrameTime Frame { get; }
    public Guid NodeId { get; }
    public string ElementName { get; }
    public int Id { get; set; }
    public Action OnRendered { get; set; }

    public PreviewRequest(int id, VecI size, KeyFrameTime frame, Guid nodeId, string elementName, Action onRendered)
    {
        Id = id;
        Size = size;
        Frame = frame;
        NodeId = nodeId;
        ElementName = elementName;
        OnRendered = onRendered;
    }
}
