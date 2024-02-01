using PixiEditor.Extensions.CommonApi.LayoutBuilding;
using PixiEditor.Extensions.CommonApi.LayoutBuilding.Events;
using PixiEditor.Extensions.IO;
using PixiEditor.Extensions.LayoutBuilding.Elements;

namespace PixiEditor.DevTools.Layouts;

public class LivePreviewWindowState : State
{
    public string? SelectedExtension { get; set; }
    public override LayoutElement BuildElement()
    {
        return new Button(
            child: SelectedExtension != null ? new Text($"Selected extension: {SelectedExtension}") : new Text("Select extension"),
            onClick: OnClick);
    }

    private void OnClick(ElementEventArgs args)
    {
        if (DevToolsExtension.PixiEditorApi.FileSystem.OpenFileDialog(new FileFilter().AddFilter("Extension Metadata file", "*.json"), out string? path))
        {
            SetState(() =>
            {
                SelectedExtension = path;
            });
        }
    }
}
