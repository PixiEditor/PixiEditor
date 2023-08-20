using System.Collections.Generic;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace PixiEditor.AvaloniaUI.ViewModels.Dock;

public class DockFactory : Factory
{
    private DockDock mainLayout;
    private DocumentDock documentDock;
    private ToolDock toolDock;

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
        };
        return base.CreateLayout();
    }

    private DockDock BuildMainLayout()
    {
        DockDock dock = new DockDock()
        {
            Name = "MainLayout",
            Id = "MainLayout",
            VisibleDockables = BuildDockables(),
        };

        return dock;
    }

    private IList<IDockable>? BuildDockables()
    {
        List<IDockable> dockables = new List<IDockable>();
        dockables.Add(BuildToolDock());

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
                        BuildDocumentDock(),
                        BuildPropertiesDock()
                    },
                },
            },
        };

        dockables.Add(BuildDocumentDock());

        return dockables;
    }

    private IDockable BuildDocumentDock()
    {
        documentDock = new DocumentDock()
        {
            Id = "DocumentsPane",
            Title = "Document",
            CanCreateDocument = true,
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

        HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>()
        {
            { "MainLayout", () => new HostWindow() },
        };

        base.InitLayout(layout);
    }
}
