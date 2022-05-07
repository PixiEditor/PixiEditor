using System;
using ChunkyImageLib.DataHolders;
using PixiEditorPrototype.ViewModels;

namespace PixiEditorPrototype.Models;
internal class DocumentStateHandler
{
    private DocumentViewModel owner;

    private Tool activeTool = Tool.Rectangle;
    private bool updateableChangeActive = false;
    private bool transformActive = false;

    public DocumentStateHandler(DocumentViewModel owner)
    {
        this.owner = owner;
    }

    public void OnMouseDown(Vector2i position)
    {

    }

    public void ChangeTool(Tool newTool)
    {
        if (updateableChangeActive)
            EndUpdateableChange();
        activeTool = newTool;
    }

    private void EndUpdateableChange()
    {
        if (!updateableChangeActive)
            throw new InvalidOperationException("No updateable change active");
        updateableChangeActive = false;
    }
}
