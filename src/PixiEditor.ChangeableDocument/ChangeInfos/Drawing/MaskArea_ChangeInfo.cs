using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.ChangeableDocument.ChangeInfos.Drawing;

public record class MaskArea_ChangeInfo(Guid Id, AffectedArea Area) : IChangeInfo;
