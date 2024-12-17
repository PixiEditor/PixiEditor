namespace PixiEditor.SVG.Units;

public interface ISvgUnit
{
    public string ToXml();
    public void ValuesFromXml(string readerValue);
}
