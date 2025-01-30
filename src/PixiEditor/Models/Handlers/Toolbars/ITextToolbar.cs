using Drawie.Backend.Core.Text;

namespace PixiEditor.Models.Handlers.Toolbars;

internal interface ITextToolbar : IFillableShapeToolbar
{
    public double FontSize { get; set; }
    public FontFamilyName? FontFamily { get; set; }
    
    public Font ConstructFont();
}
