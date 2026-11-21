namespace PixiEditor.ChangeableDocument.ChangeInfos.Animation;

public record CreateCelKeyFrame_ChangeInfo(
    Guid TargetLayerGuid,
    int Frame,
    Guid KeyFrameId,
    bool CloneFromExisting) : IChangeInfo;
