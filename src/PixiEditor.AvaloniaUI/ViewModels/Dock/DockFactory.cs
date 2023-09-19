using System.Collections.Generic;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using PixiEditor.AvaloniaUI.ViewModels.SubViewModels;

namespace PixiEditor.AvaloniaUI.ViewModels.Dock;

internal class DockFactory : Factory
{
    private DockDock mainLayout;
    private DocumentDock documentDock;
    private ToolDock toolDock;

    private FileViewModel manager;

    public DockFactory(FileViewModel fileViewModel)
    {
        manager = fileViewModel;
    }

    public override IDocumentDock CreateDocumentDock() => new PixiEditorDocumentDock(manager);

    public override IRootDock CreateLayout()
    {
        mainLayout = BuildMainLayout();
        RootDock root = new()
        {
            Id = "Root",
            IsCollapsable = false,
            VisibleDockables = new List<IDockable>()
            {
                mainLayout,
            },
            ActiveDockable = mainLayout
        };
        return root;
    }

    private DockDock BuildMainLayout()
    {
        var dockables = BuildDockables();
        DockDock dock = new DockDock()
        {
            Name = "MainLayout",
            Id = "MainLayout",
            VisibleDockables = dockables,
            ActiveDockable = dockables[0],
        };

        return dock;
    }

    private IList<IDockable>? BuildDockables()
    {
        List<IDockable> dockables = new List<IDockable>();

        IDockable documentDock = BuildDocumentDock();

        ProportionalDock topPane = new ProportionalDock()
        {
            Id = "TopPane",
            Orientation = Orientation.Vertical,
            VisibleDockables = new List<IDockable>()
            {
                new ProportionalDock()
                {
                    Id = "RightPane",
                    Orientation = Orientation.Horizontal,
                    VisibleDockables = new List<IDockable>()
                    {
                        documentDock,
                        BuildPropertiesDock()
                    },
                    ActiveDockable = documentDock
                },
            },
        };

        dockables.Add(topPane);

        return dockables;
    }

    private IDockable BuildDocumentDock()
    {
        documentDock = new PixiEditorDocumentDock(manager)
        {
            Id = "DocumentsPane",
            Title = "DocumentsPane",
            CanCreateDocument = true
        };

        return documentDock;
    }

    private IDockable BuildPropertiesDock()
    {
        IDockable layersDock = BuildLayersDock();
        return new ProportionalDock()
        {
            Proportion = 0.15,
            VisibleDockables = CreateList(layersDock),
            ActiveDockable = layersDock,
        };
    }

    private IDockable BuildLayersDock()
    {
        LayersDockViewModel layersDock = new()
        {
            Id = "LayersPane",
            Title = "LayersPane",
        };

        ToolDock layers = new()
        {
            Id = "LayersPane",
            Title = "LayersPane",
            VisibleDockables = new List<IDockable>() { layersDock },
            ActiveDockable = layersDock,
        };

        return layers;
    }

    public override void InitLayout(IDockable layout)
    {
        // Uhh, don't ask me what to put here, I just copied from the example
        DockableLocator = new Dictionary<string, Func<IDockable?>>()
        {
            { "MainLayout", () => mainLayout },
            { "DocumentsPane", () => documentDock },
            { "ToolsPane", () => toolDock },
        };

        ContextLocator = new Dictionary<string, Func<object?>>()
        {
            { "MainLayout", () => layout },
            { "ToolsPane", () => layout },
        };

        HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>()
        {
            [nameof(IDockWindow)] = () => new HostWindow()
        };

        base.InitLayout(layout);
    }
}
