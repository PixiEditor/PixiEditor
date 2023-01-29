using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.ChangeableDocument.ChangeInfos.Drawing;

public record class MaskArea_ChangeInfo(Guid GuidValue, AffectedArea Area) : IChangeInfo;
