namespace PixiEditor.ChangeableDocument.ChangeInfos.Animation;

public record KeyFrameVisibility_ChangeInfo(Guid KeyFrameId, bool IsVisible) : IChangeInfo;
