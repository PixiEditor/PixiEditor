using PixiEditor.Extensions.CommonApi.LayoutBuilding.Events;
using PixiEditor.Extensions.IO;
using PixiEditor.Extensions.LayoutBuilding.Elements;
using PixiEditor.Extensions.Runtime;

namespace PixiEditor.DevTools.Layouts;

public class LivePreviewWindowState : State
{
    private ExtensionLoader Loader { get; }
    public string? SelectedLayoutFile { get; set; }

    public LivePreviewWindowState()
    {
        Loader = new ExtensionLoader(AppDomain.CurrentDomain.BaseDirectory);
    }

    public override LayoutElement BuildElement()
    {
        return new Align(
            alignment: Alignment.TopLeft,
            child: new Column(
                children: new Button(
                    child: SelectedLayoutFile != null ? new Text($"Selected extension: {SelectedLayoutFile}") : new Text("Select extension"),
                    onClick: OnClick)
                )
            );
    }

    private void OnClick(ElementEventArgs args)
    {
        if (DevToolsExtension.PixiEditorApi.FileSystem.OpenFileDialog(new FileFilter().AddFilter("Layout C# Script", "*.cs"), out string? path))
        {
            SetState(() =>
            {
                SelectedLayoutFile = path;

            });
        }
    }
}
