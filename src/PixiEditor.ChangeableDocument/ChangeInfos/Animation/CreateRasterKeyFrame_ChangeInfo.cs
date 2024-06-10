namespace PixiEditor.ChangeableDocument.ChangeInfos.Animation;

public record CreateRasterKeyFrame_ChangeInfo(
    Guid TargetLayerGuid,
    int Frame,
    int IndexOfCreatedClip,
    bool CloneFromExisting) : IChangeInfo;
