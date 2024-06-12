namespace PixiEditor.ChangeableDocument.Changeables.Animations;

public class GroupKeyFrame : KeyFrame
{
    public List<KeyFrame> Children { get; } = new List<KeyFrame>();
    public GroupKeyFrame(Guid layerGuid, int startFrame) : base(layerGuid, startFrame)
    {
        Id = layerGuid;
    }
}
