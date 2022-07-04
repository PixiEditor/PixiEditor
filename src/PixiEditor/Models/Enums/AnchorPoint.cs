using System;

namespace PixiEditor.Models.Enums;

[Flags]
public enum AnchorPoint
{
    Left = 1,
    Center = 2,
    Right = 4,
    Top = 8,
    Middle = 16,
    Bottom = 32
}