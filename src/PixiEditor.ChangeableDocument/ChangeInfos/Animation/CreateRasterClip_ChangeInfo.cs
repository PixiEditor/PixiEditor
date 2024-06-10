namespace PixiEditor.ChangeableDocument.ChangeInfos.Animation;

public record CreateRasterClip_ChangeInfo(
    Guid TargetLayerGuid,
    int Frame,
    int IndexOfCreatedClip,
    bool CloneFromExisting) : IChangeInfo;
