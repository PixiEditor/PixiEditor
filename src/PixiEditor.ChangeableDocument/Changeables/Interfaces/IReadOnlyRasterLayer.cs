namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IReadOnlyRasterLayer : ITransparencyLockable
{
    /// <summary>
    /// The chunky image of the layer
    /// </summary>
    IReadOnlyChunkyImage GetLayerImageAtFrame(int frame);

    public IReadOnlyChunkyImage GetLayerImageByKeyFrameGuid(Guid keyFrameGuid);
    void SetLayerImageAtFrame(int frame, IReadOnlyChunkyImage image);
    public void ForEveryFrame(Action<IReadOnlyChunkyImage> action);
}
