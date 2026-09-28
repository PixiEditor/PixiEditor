namespace PixiEditor.ChangeableDocument.ChangeInfos.Structure;

public record LayerLock_ChangeInfo(Guid Layer, bool IsLocked) : IChangeInfo
{

}
