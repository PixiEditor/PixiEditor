using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.ChangeInfos.Root;

public record class Size_ChangeInfo(VecI Size, double VerticalSymmetryAxisX, double HorizontalSymmetryAxisY) : IChangeInfo;
