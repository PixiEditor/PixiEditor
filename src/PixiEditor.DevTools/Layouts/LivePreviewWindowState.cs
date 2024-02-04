using Microsoft.CodeAnalysis.MSBuild;
using PixiEditor.DevTools.CsharpCoding;
using PixiEditor.Extensions.CommonApi.LayoutBuilding.Events;
using PixiEditor.Extensions.IO;
using PixiEditor.Extensions.LayoutBuilding.Elements;
using PixiEditor.Extensions.Runtime;

namespace PixiEditor.DevTools.Layouts;

public class LivePreviewWindowState : State
{
    private ExtensionLoader Loader { get; }
    private ProjectLoader projectLoader;
    private ProjectCompiler compiler;
    private HotReloader reloader;

    private LayoutElement? _element;
    public string? SelectedProjectFile { get; set; }

    public LivePreviewWindowState()
    {
        Loader = new ExtensionLoader(AppDomain.CurrentDomain.BaseDirectory);
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
        compiler.Compile();
        SetState(BuildLayoutElement);
    }

    private void BuildLayoutElement()
    {
        var typeToInit = compiler.LayoutElementTypes.FirstOrDefault();
        if (typeToInit != null)
        {
            var instance = (LayoutElement)Activator.CreateInstance(typeToInit);
            _element = instance;
        }
    }

    private void OnClick(ElementEventArgs args)
    {
        if (DevToolsExtension.PixiEditorApi.FileSystem.OpenFileDialog(new FileFilter().AddFilter("C# project file", "*.csproj"), out string? path))
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
        projectLoader = new ProjectLoader(SelectedProjectFile);
        projectLoader.LoadProjects();
        compiler = new ProjectCompiler(projectLoader.Workspace, projectLoader.AllProjects);
        reloader.WatchProject(SelectedProjectFile);
        compiler.Compile();
        BuildLayoutElement();
    }
}
