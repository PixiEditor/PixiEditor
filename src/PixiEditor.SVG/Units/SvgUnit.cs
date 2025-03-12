using PixiEditor.SVG.Elements;

namespace PixiEditor.SVG.Units;

public interface ISvgUnit
{
    public string ToXml(DefStorage defs);
    public void ValuesFromXml(string readerValue, SvgDefs defs);
}
