namespace PixiEditor.ChangeableDocument.ChangeInfos.Root;

public record class Size_ChangeInfo(VecI Size, int VerticalSymmetryAxisX, int HorizontalSymmetryAxisY) : IChangeInfo;
