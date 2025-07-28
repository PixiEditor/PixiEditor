using Drawie.Backend.Core.Vector;

namespace PixiEditor.ChangeableDocument.ChangeInfos.Drawing;

public record class Selection_ChangeInfo(VectorPath NewPath) : IChangeInfo;
