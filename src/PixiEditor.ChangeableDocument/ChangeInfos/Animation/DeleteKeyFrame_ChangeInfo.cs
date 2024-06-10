namespace PixiEditor.ChangeableDocument.ChangeInfos.Animation;

public record DeleteKeyFrame_ChangeInfo(
    int Frame,
    int IndexOfDeletedClip) : IChangeInfo;
