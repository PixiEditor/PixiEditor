namespace PixiEditor.ChangeableDocument.ChangeInfos.Animation;

public record KeyFrameLength_ChangeInfo(Guid KeyFrameGuid, int StartFrame, int Duration) : IChangeInfo;
