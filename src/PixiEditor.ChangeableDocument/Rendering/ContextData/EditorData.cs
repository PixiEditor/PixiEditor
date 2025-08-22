using Drawie.Backend.Core.ColorsImpl;

namespace PixiEditor.ChangeableDocument.Rendering.ContextData;

public struct EditorData
{
    public Color PrimaryColor { get; }
    public Color SecondaryColor { get; }

    public EditorData(Color primaryColor, Color secondaryColor)
    {
        PrimaryColor = primaryColor;
        SecondaryColor = secondaryColor;
    }
}
