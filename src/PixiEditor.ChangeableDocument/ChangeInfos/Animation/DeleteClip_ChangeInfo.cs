namespace PixiEditor.ChangeableDocument.ChangeInfos.Animation;

public record DeleteClip_ChangeInfo(
    int Frame,
    int IndexOfDeletedClip) : IChangeInfo;
