using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Animations;

public abstract class KeyFrame : IReadOnlyKeyFrame
{
    public int StartFrame { get; set; }
    public int Duration { get; set; }
    public Guid LayerGuid { get; }
    public Guid Id { get; set; }

    protected KeyFrame(Guid layerGuid, int startFrame)
    {
        LayerGuid = layerGuid;
        StartFrame = startFrame;
        Duration = 1;
        Id = Guid.NewGuid();
    }

    public virtual void ActiveFrameChanged(int atFrame) { }
    public virtual void Deactivated(int atFrame) { }
}
