using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PixiDocks.Avalonia;
using PixiDocks.Avalonia.Controls;
using PixiDocks.Core.Docking;
using PixiDocks.Core.Serialization;
using PixiEditor.ViewModels.Document;
using PixiEditor.ViewModels.SubViewModels;
using PixiEditor.Views.Main;

namespace PixiEditor.ViewModels.Dock;

internal class LayoutManager
{
    public LayoutTree DefaultLayout { get; set; }

    public LayoutTree ActiveLayout { get; set; }

    public DockContext DockContext { get; set; } = new DockContext();

    public IReadOnlyCollection<IDockableContent> RegisteredDockables => registeredDockables;
    public event Action<HostWindow> WindowFloated;

    private readonly List<IDockableContent> registeredDockables = new();

    public LayoutManager()
    {
    }

    public void InitLayout(ViewModelMain mainViewModel)
    {
        LayersDockViewModel layersDockViewModel = new(mainViewModel.DocumentManagerSubViewModel);
        ColorPickerDockViewModel colorPickerDockViewModel = new(mainViewModel.ColorsSubViewModel);
        ColorSlidersDockViewModel colorSldersDockViewModel = new(mainViewModel.ColorsSubViewModel);
        DocumentPreviewDockViewModel documentPreviewDockViewModel =
            new(mainViewModel.ColorsSubViewModel, mainViewModel.DocumentManagerSubViewModel);
        SwatchesDockViewModel swatchesDockViewModel = new(mainViewModel.DocumentManagerSubViewModel);
        PaletteViewerDockViewModel paletteViewerDockViewModel =
            new(mainViewModel.ColorsSubViewModel, mainViewModel.DocumentManagerSubViewModel);
        TimelineDockViewModel timelineDockViewModel = new(mainViewModel.DocumentManagerSubViewModel);

        NodeGraphDockViewModel nodeGraphDockViewModel = new(mainViewModel.DocumentManagerSubViewModel);
        /*
        ChannelsDockViewModel channelsDockDockViewModel = new(mainViewModel.WindowSubViewModel);
        */

        RegisterDockable(layersDockViewModel);
        RegisterDockable(colorPickerDockViewModel);
        RegisterDockable(colorSldersDockViewModel);
        RegisterDockable(documentPreviewDockViewModel);
        RegisterDockable(swatchesDockViewModel);
        RegisterDockable(paletteViewerDockViewModel);
        RegisterDockable(timelineDockViewModel);
        RegisterDockable(nodeGraphDockViewModel);
        /*
        RegisterDockable(channelsDockDockViewModel);
        */

        DefaultLayout = new LayoutTree
        {
            Root = new DockableTree
            {
                First = new DockableTree()
                {
                    First = new DockableArea()
                    {
                        Id = "DocumentArea", FallbackContent = new CreateDocumentFallbackView(),
                    },
                    SplitDirection = DockingDirection.Bottom,
                    SecondSize = 300,
                    AutoExpand = true,
                    Second = new DockableArea() { Id = "TimelineArea", CloseRegionOnEmpty = false }
                },
                SecondSize = 360,
                SplitDirection = DockingDirection.Right,
                Second = new DockableTree
                {
                    Id = "PropertiesArea",
                    First = new DockableTree
                    {
                        First = new DockableArea
                        {
                            Id = "ColorsArea",
                            Dockables =
                            [
                                DockContext.CreateDockable(colorPickerDockViewModel),
                                DockContext.CreateDockable(colorSldersDockViewModel),
                                DockContext.CreateDockable(swatchesDockViewModel),
                                DockContext.CreateDockable(paletteViewerDockViewModel)
                            ]
                        },
                        FirstSize = 0.6,
                        SplitDirection = DockingDirection.Bottom,
                        Second = new DockableArea
                        {
                            Id = "LayersArea", Dockables = [DockContext.CreateDockable(layersDockViewModel)]
                        },
                    },
                    FirstSize = 0.66,
                    SplitDirection = DockingDirection.Bottom,
                    Second = new DockableArea
                    {
                        Id = "DocumentPreviewArea",
                        ActiveDockable = DockContext.CreateDockable(documentPreviewDockViewModel)
                    }
                }
            }
        };

        ActiveLayout = DefaultLayout;
        ActiveLayout.SetContext(DockContext);

        DockContext.WindowFloated += WindowFloated;
    }

    private IDockable? TryCreateDockable(string name)
    {
        var foundDockable = RegisteredDockables.FirstOrDefault(x => x.Id == name);
        if (foundDockable != null)
        {
            return DockContext.CreateDockable(foundDockable);
        }

        return null;
    }

    public void RegisterDockable(IDockableContent dockable)
    {
        if (registeredDockables.Contains(dockable))
        {
            return;
        }

        registeredDockables.Add(dockable);
    }

    public void UnregisterDockable(IDockableContent dockable)
    {
        registeredDockables.Remove(dockable);
    }

    public void AddViewport(IDockableContent viewport)
    {
        RegisterDockable(viewport);
        DockableArea? documentsArea = TryFindArea("DocumentArea");
        IDockable dockable = DockContext.CreateDockable(viewport);
        if (documentsArea != null)
        {
            documentsArea.AddDockable(dockable);
            documentsArea.ActiveDockable = dockable;
        }
        else
        {
            DockContext.Float(dockable, 0, 0);
        }
    }

    public void ShowViewport(ViewportWindowViewModel viewport)
    {
        foreach (var element in ActiveLayout.Root)
        {
            if (element is IDockableHost dockableHost)
            {
                var dockable = dockableHost.Dockables.FirstOrDefault(x => x.Id == viewport.Id);
                if (dockable != null)
                {
                    dockableHost.ActiveDockable = dockable;
                    return;
                }
            }
        }
    }

    private DockableArea? TryFindArea(string name)
    {
        DockableArea? result = null;
        foreach (var element in ActiveLayout.Root)
        {
            if (element is DockableArea area && area.Id == name)
            {
                result = area;
            }
        }

        return result;
    }

    public void RemoveViewport(IDockableContent content)
    {
        foreach (var element in ActiveLayout.Root)
        {
            if (element is IDockableHost dockableHost)
            {
                var dockable = dockableHost.Dockables.FirstOrDefault(x => x.Id == content.Id);
                if (dockable != null)
                {
                    dockableHost?.RemoveDockable(dockable);
                    UnregisterDockable(content);
                    return;
                }
            }
        }
    }

    public void ShowDockable(string id)
    {
        foreach (var element in ActiveLayout.Root)
        {
            if (element is IDockableHost dockableHost)
            {
                var dockable = dockableHost.Dockables.FirstOrDefault(x => x.Id == id);
                if (dockable != null)
                {
                    dockableHost.ActiveDockable = dockable;
                    return;
                }
            }
        }

        IDockable? created = TryCreateDockable(id);
        if (created != null)
        {
            bool attached = false;
            ActiveLayout.Root.Traverse(((element, tree) =>
            {
                if (element is IDockableHost host)
                {
                    if (element.Id == $"{id}Area" && !attached)
                    {
                        host.AddDockable(created);
                        host.ActiveDockable = created;
                        attached = true;
                    }
                    else if (id == NodeGraphDockViewModel.TabId && element.Id == "DocumentArea")
                    {
                        host.AddDockable(created);
                        host.ActiveDockable = created;
                        attached = true;
                    }
                }
            }));

            if (!attached)
            {
                DockContext.Float(created, 0, 0);
            }
        }
    }
}
