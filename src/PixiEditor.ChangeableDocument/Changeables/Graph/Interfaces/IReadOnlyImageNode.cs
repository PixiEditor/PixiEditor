using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface IReadOnlyImageNode : IReadOnlyLayerNode, ITransparencyLockable
{
    /// <summary>
    /// The chunky image of the layer
    /// </summary>
    IReadOnlyChunkyImage GetLayerImageAtFrame(int frame);

    public IReadOnlyChunkyImage GetLayerImageByKeyFrameGuid(Guid keyFrameGuid);
    void SetLayerImageAtFrame(int frame, IReadOnlyChunkyImage image);
    public void ForEveryFrame(Action<IReadOnlyChunkyImage> action);
}
