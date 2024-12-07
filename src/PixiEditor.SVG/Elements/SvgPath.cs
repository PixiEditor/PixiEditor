﻿using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Elements;

public class SvgPath() : SvgPrimitive("path")
{
    public SvgProperty<SvgStringUnit> PathData { get; } = new("d");
}
