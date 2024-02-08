using PixiEditor.Extensions.CommonApi.LayoutBuilding.Events;
using PixiEditor.Extensions.IO;
using PixiEditor.Extensions.LayoutBuilding.Elements;
using PixiEditor.Extensions.Runtime;

namespace PixiEditor.DevTools.Layouts;

public class LivePreviewWindowState : State
{
    private HotReloader reloader;

    private LayoutElement? _element;
    private LayoutDeserializer? _deserializer;
    public string? SelectedProjectFile { get; set; }

    public LivePreviewWindowState()
    {
        reloader = new HotReloader();
        reloader.OnFileChanged += OnFileChanged;
    }

    public override LayoutElement BuildElement()
    {
        return new Align(
            alignment: Alignment.TopLeft,
            child: new Column(
                new Button(
                    child: SelectedProjectFile != null ? new Text($"Selected extension: {SelectedProjectFile}") : new Text("Select extension"),
                    onClick: OnClick),
                _element ?? new Text("No layout element selected")
                )
            );
    }

    private void OnFileChanged(string obj)
    {
        SetState(BuildLayoutElement);
    }

    private void BuildLayoutElement()
    {
        if (_deserializer != null)
        {
            _element = _deserializer.DeserializeLayout();
        }
    }

    private void OnClick(ElementEventArgs args)
    {
        if (DevToolsExtension.PixiEditorApi.FileSystem.OpenFileDialog(new FileFilter().AddFilter("Layout file", "*.layout"), out string? path))
        {
            SetState(() =>
            {
                SelectedProjectFile = path;
                InitProject();
            });
        }
    }

    private void InitProject()
    {
        _deserializer = new LayoutDeserializer(SelectedProjectFile);
        reloader.WatchFile(SelectedProjectFile, "*.layout");
        BuildLayoutElement();
    }
}
