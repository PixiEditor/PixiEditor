using System.ComponentModel;

namespace PixiEditor.ChangeableDocument.Enums;
public enum SelectionMode
{
    [Description("NEW")]
    New,
    [Description("ADD")]
    Add,
    [Description("SUBTRACT")]
    Subtract,
    [Description("INTERSECT")]
    Intersect
}
