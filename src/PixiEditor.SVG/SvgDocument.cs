using PixiEditor.Numerics;

namespace PixiEditor.SVG;

public class SvgDocument
{
   public string RootNamespace { get; set; } = "http://www.w3.org/2000/svg";
   public string Version { get; set; } = "1.1";
   public RectD ViewBox { get; set; }
}
