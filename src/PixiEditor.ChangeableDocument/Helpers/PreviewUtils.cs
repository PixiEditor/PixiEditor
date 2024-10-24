using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Helpers;

public static class PreviewUtils
{
    public static RectD? FindPreviewBounds(IOutputProperty? connectionProperty, int frame, string elementToRenderName)
    {
        if (connectionProperty is { Node: IPreviewRenderable previousPreview })
        {
            return previousPreview.GetPreviewBounds(frame, elementToRenderName);
        }

        return null;
    }
}
