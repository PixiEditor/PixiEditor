using Drawie.Backend.Core.Text;
using PixiEditor.ChangeableDocument.Changeables;

namespace PixiEditor.Models.Handlers.Toolbars;

internal interface ITextToolbar : IFillableShapeToolbar
{
    public double FontSize { get; set; }
    public FontFamilyName FontFamily { get; set; }
    public FontStyle FontStyle { get; set; }
    public double Spacing { get; set; }
    public bool ForceLowDpiRendering { get; set; }
    public bool Bold { get; set; }
    public bool Italic { get; set; }
    public TextAlign Alignment { get; set; }
    public FontData ConstructFont();
    void UpdateFontStyles();
}
