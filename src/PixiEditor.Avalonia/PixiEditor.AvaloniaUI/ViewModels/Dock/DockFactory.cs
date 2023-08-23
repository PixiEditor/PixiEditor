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
        dockables.Add(BuildToolDock());

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
        return new ProportionalDock();

    }

    private IDockable BuildToolDock()
    {
        toolDock = new ToolDock()
        {
            Dock = DockMode.Left,
            CanFloat = false,
            GripMode = GripMode.Hidden,
            Id = "ToolsPane",
            Title = "ToolsPane",
        };

        return toolDock;
    }

    public override void InitLayout(IDockable layout)
    {
        DockableLocator = new Dictionary<string, Func<IDockable?>>()
        {
            { "MainLayout", () => mainLayout },
            { "DocumentsPane", () => documentDock },
            { "ToolsPane", () => toolDock },
        };

        ContextLocator = new Dictionary<string, Func<object?>>()
        {
            { "MainLayout", () => layout },
            { "DocumentsPane", () => layout },
            { "ToolsPane", () => layout },
        };

        HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>()
        {
            [nameof(IDockWindow)] = () => new HostWindow()
        };

        base.InitLayout(layout);
    }
}
