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
    private ToolDock layersDock;
    private ToolDock colorPickerDock;
    private ToolDock navigationDock;

    private FileViewModel fileVm;
    private ColorsViewModel colorsVm;

    public DockFactory(FileViewModel fileViewModel, ColorsViewModel colorsViewModel)
    {
        fileVm = fileViewModel;
        colorsVm = colorsViewModel;
    }

    public override IDocumentDock CreateDocumentDock() => new PixiEditorDocumentDock(fileVm);

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
        documentDock = new PixiEditorDocumentDock(fileVm)
        {
            Id = "DocumentsPane",
            Title = "DocumentsPane",
            CanCreateDocument = true
        };

        return documentDock;
    }

    private IDockable BuildPropertiesDock()
    {
        layersDock = BuildLayersDock();
        colorPickerDock = BuildColorPickerDock();
        navigationDock = BuildNavigationDock();
        return new ProportionalDock()
        {
            Proportion = 0.20,
            Orientation = Orientation.Vertical,
            VisibleDockables = new List<IDockable>()
            {
                colorPickerDock,
                layersDock,
                navigationDock,
            },
            ActiveDockable = layersDock,
        };
    }

    private ToolDock BuildNavigationDock()
    {
        NavigationDockViewModel navigationVm = new(colorsVm, fileVm.Owner.DocumentManagerSubViewModel)
        {
            Id = "NavigationPane",
            Title = "NavigationPane",
        };

        ToolDock navigation = new()
        {
            Id = "NavigationPane",
            Title = "NavigationPane",
            VisibleDockables = new List<IDockable>() { navigationVm },
            ActiveDockable = navigationVm,
        };

        return navigation;
    }

    private ToolDock BuildColorPickerDock()
    {
        ColorPickerDockViewModel colorPickerVm = new(colorsVm)
        {
            Id = "ColorPickerPane",
            Title = "ColorPickerPane",
        };

        PaletteViewerDockViewModel paletteViewerVm = new(colorsVm, fileVm.Owner.DocumentManagerSubViewModel)
        {
            Id = "PaletteViewerPane",
            Title = "PaletteViewerPane",
        };

        SwatchesDockViewModel swatchesVm = new(fileVm.Owner.DocumentManagerSubViewModel)
        {
            Id = "SwatchesPane",
            Title = "SwatchesPane",
        };

        ToolDock colorPicker = new()
        {
            Id = "ColorPickerPane",
            Title = "ColorPickerPane",
            VisibleDockables = new List<IDockable>() { colorPickerVm, paletteViewerVm, swatchesVm },
            ActiveDockable = colorPickerVm,
        };

        return colorPicker;
    }

    private ToolDock BuildLayersDock()
    {
        LayersDockViewModel layersVm = new(fileVm.Owner.DocumentManagerSubViewModel)
        {
            Id = "LayersPane",
            Title = "LayersPane",
        };

        ToolDock layers = new()
        {
            Id = "LayersPane",
            Title = "LayersPane",
            VisibleDockables = new List<IDockable>() { layersVm },
            ActiveDockable = layersVm,
        };

        return layers;
    }

    public override void InitLayout(IDockable layout)
    {
        DockableLocator = new Dictionary<string, Func<IDockable?>>()
        {
            { "MainLayout", () => mainLayout },
            { "DocumentsPane", () => documentDock },
            { "ToolsPane", () => toolDock },
            { "LayersPane", () => layersDock },
        };

        ContextLocator = new Dictionary<string, Func<object?>>()
        {
            { "MainLayout", () => layout },
            { "ToolsPane", () => layout },
            { "DocumentsPane", () => layout },
            { "LayersPane", () => layout },
        };

        HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>()
        {
            [nameof(IDockWindow)] = () => new HostWindow()
        };

        base.InitLayout(layout);
    }
}
