using SkiaSharp;

namespace PixiEditor.ChangeableDocument.ChangeInfos.Drawing;

public record class Selection_ChangeInfo(SKPath NewPath) : IChangeInfo;
