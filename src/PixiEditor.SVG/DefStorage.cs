using PixiEditor.SVG.Elements;

namespace PixiEditor.SVG;

public class DefStorage
{
    public SvgDocument Root { get; }
    public SvgDefs? Defs { get; private set; }

    private int _idCounter = 0;
    public DefStorage(SvgDocument root)
    {
        Root = root;
    }

    public void AddDef(SvgElement def)
    {
        if (Defs == null)
        {
            Defs = new SvgDefs();
            Root.Defs = Defs;
        }

        Defs.Children.Add(def);
        _idCounter++;
    }

    public string GetNextId()
    {
        return _idCounter.ToString();
    }
}

