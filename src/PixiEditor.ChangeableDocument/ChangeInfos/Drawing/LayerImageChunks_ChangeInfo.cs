namespace PixiEditor.ChangeableDocument.ChangeInfos.Drawing;

public record class LayerImageChunks_ChangeInfo(Guid GuidValue, HashSet<VecI> Chunks) : IChangeInfo;
