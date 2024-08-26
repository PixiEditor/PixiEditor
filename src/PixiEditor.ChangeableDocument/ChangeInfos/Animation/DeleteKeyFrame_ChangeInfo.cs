namespace PixiEditor.ChangeableDocument.ChangeInfos.Animation;

public record DeleteKeyFrame_ChangeInfo(
    Guid DeletedKeyFrameId) : IChangeInfo;
