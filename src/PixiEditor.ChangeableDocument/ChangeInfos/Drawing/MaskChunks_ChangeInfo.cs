namespace PixiEditor.ChangeableDocument.ChangeInfos.Drawing;

public record class MaskChunks_ChangeInfo(Guid GuidValue, HashSet<VecI> Chunks) : IChangeInfo;
