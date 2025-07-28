namespace PixiEditor.ChangeableDocument.ChangeInfos.Animation;

public record CreateRasterKeyFrame_ChangeInfo(
    Guid TargetLayerGuid,
    int Frame,
    Guid KeyFrameId,
    bool CloneFromExisting) : IChangeInfo;
