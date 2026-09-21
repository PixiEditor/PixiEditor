using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PixiDocks.Avalonia;
using PixiDocks.Avalonia.Controls;
using PixiDocks.Core.Docking;
using PixiDocks.Core.Serialization;
using PixiEditor.Extensions.CommonApi.UserPreferences;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;
using PixiEditor.UI.Common.Behaviors;
using PixiEditor.ViewModels.SubViewModels;
using PixiEditor.Views.Main;

namespace PixiEditor.ViewModels.Dock;

internal class LayoutManager
{
    public LayoutTree DefaultLayout { get; set; }

    public LayoutTree ActiveLayout { get; set; }

    public DockContext DockContext { get; set; }

    public IReadOnlyCollection<IDockableContent> RegisteredDockables => registeredDockables;
    public event Action<HostWindow> WindowFloated;

    public event Action<DockableTree> LayoutReplaced;

    private double Scaling => scaling;

    private double scaling = 1;

    private readonly List<IDockableContent> registeredDockables = new();

    private List<HostWindow> floatedWindows = new();

    private static readonly JsonSerializerOptions LayoutJsonOptions = new() { WriteIndented = false };
    private LayersDockViewModel layersDockViewModel;
    private ColorPickerDockViewModel colorPickerDockViewModel;
    private ColorSlidersDockViewModel colorSldersDockViewModel;
    private DocumentPreviewDockViewModel documentPreviewDockViewModel;
    private SwatchesDockViewModel swatchesDockViewModel;
    private PaletteViewerDockViewModel paletteViewerDockViewModel;

    public LayoutManager()
    {
        PixiEditorSettings.Accessibility.UiScaleFactor.ValueChanged += (setting, value) =>
        {
            scaling = value;
            foreach (var window in floatedWindows)
            {
                UpdateHostWindowScaling(window);
            }
        };

        scaling = PixiEditorSettings.Accessibility.UiScaleFactor.Value;

        DockContext = new DockContext()
        {
            HostWindowFactory = () =>
            {
                var hostWindow = new HostWindow();
                UpdateHostWindowScaling(hostWindow);
                return hostWindow;
            }
        };
    }

    private void UpdateHostWindowScaling(HostWindow hostWindow)
    {
        Dispatcher.UIThread.Post(() =>
        {
            LayoutTransformControl transformControl = hostWindow.FindDescendantOfType<LayoutTransformControl>();
            transformControl.LayoutTransform = new ScaleTransform(Scaling, Scaling);
        });
    }

    public void InitLayout(ViewModelMain mainViewModel)
    {
        layersDockViewModel = new(mainViewModel.DocumentManagerSubViewModel);
        colorPickerDockViewModel = new(mainViewModel.ColorsSubViewModel);
        colorSldersDockViewModel = new(mainViewModel.ColorsSubViewModel);
        documentPreviewDockViewModel =
            new(mainViewModel.ColorsSubViewModel, mainViewModel.DocumentManagerSubViewModel);
        swatchesDockViewModel = new(mainViewModel.DocumentManagerSubViewModel);
        paletteViewerDockViewModel =
            new(mainViewModel.ColorsSubViewModel, mainViewModel.DocumentManagerSubViewModel);
        TimelineDockViewModel timelineDockViewModel = new(mainViewModel.DocumentManagerSubViewModel);

        NodeGraphDockViewModel nodeGraphDockViewModel = new(mainViewModel.DocumentManagerSubViewModel);
        /*
        ChannelsDockViewModel channelsDockDockViewModel = new(mainViewModel.WindowSubViewModel);
        */

        HostWindow.ForceUseSystemDecorations = PixiEditorSettings.Appearance.UseSystemDecorations.Value;

        PixiEditorSettings.Appearance.UseSystemDecorations.ValueChanged += (_, val) =>
        {
            HostWindow.ForceUseSystemDecorations = val;
        };

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


        DefaultLayout = BuildDefaultLayoutTree();

        string savedLayoutJson = IPreferences.Current?.GetLocalPreference<string>(PreferencesConstants.DockLayoutData, null);

        if (!string.IsNullOrWhiteSpace(savedLayoutJson) && TryBuildLayoutFromJson(savedLayoutJson, out LayoutTree restoredLayout))
        {
            ActiveLayout = restoredLayout;
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(savedLayoutJson))
            {
                IPreferences.Current?.UpdateLocalPreference<string>(PreferencesConstants.DockLayoutData, null);
            }

            ActiveLayout = DefaultLayout;
            ActiveLayout.SetContext(DockContext);
        }

        DockContext.WindowFloated += (window) =>
        {
            if (!floatedWindows.Contains(window))
            {
                floatedWindows.Add(window);
            }

            window.Closed += WindowOnClosed;

            WindowFloated?.Invoke(window);
        };


        PixiEditorSettings.Accessibility.UiScaleFactor.ValueChanged += (s, value) =>
        {
            LayoutTransformScalerBehavior.SetGlobalScaling(value);
            ColorPicker.Behaviors.LayoutTransformScalerBehavior.SetGlobalScaling(value);
        };

        LayoutTransformScalerBehavior.SetGlobalScaling(PixiEditorSettings.Accessibility.UiScaleFactor.Value);
        ColorPicker.Behaviors.LayoutTransformScalerBehavior.SetGlobalScaling(PixiEditorSettings.Accessibility
            .UiScaleFactor.Value);
    }
    public void SaveLayout()
    {
        try
        {
            string json = JsonSerializer.Serialize(ActiveLayout, LayoutJsonOptions);
            IPreferences.Current?.UpdateLocalPreference<string>(PreferencesConstants.DockLayoutData, json);
        }
        catch
        {
           
        }
    }

    public void ResetLayoutToDefault()
    {
        List<IDockable> openDocuments = new();
        foreach (var element in ActiveLayout.Root)
        {
            if (element is DockableArea area && area.Id == "DocumentArea")
            {
                foreach (var dockable in area.Dockables)
                {
                    if (dockable != null)
                    {
                        openDocuments.Add(dockable);
                    }
                }

                break;
            }
        }

        LayoutTree fresh = BuildDefaultLayoutTree();
        fresh.SetContext(DockContext);

        if (fresh.Root is DockableTree freshRoot)
        {
            foreach (var element in freshRoot)
            {
                if (element is DockableArea area && area.Id == "DocumentArea")
                {
                    foreach (var document in openDocuments)
                    {
                        area.AddDockable(document);
                    }

                    if (openDocuments.Count > 0)
                    {
                        area.ActiveDockable = openDocuments[^1];
                    }

                    break;
                }
            }
        }

        ActiveLayout = fresh;
        IPreferences.Current?.UpdateLocalPreference<string>(PreferencesConstants.DockLayoutData, null);

        if (ActiveLayout.Root is DockableTree newRoot)
        {
            LayoutReplaced?.Invoke(newRoot);
        }
    }

    private bool TryBuildLayoutFromJson(string json, out LayoutTree result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            LayoutTree parsed = JsonSerializer.Deserialize<LayoutTree>(json, LayoutJsonOptions);

            if (parsed.Root is not DockableTree treeRoot)
            {
                return false;
            }

            if (DefaultLayout.Root is DockableTree defaultRoot)
            {
                ReconcileAreaMetadata(treeRoot, defaultRoot);
            }

            parsed.SetContext(DockContext);

            List<IDockable?> liveDockables = registeredDockables
                .Select(content => DockContext.CreateDockable(content))
                .ToList();

            parsed.ApplyDockables(liveDockables);

            HashSet<string> validContentIds = registeredDockables.Select(c => c.Id).ToHashSet();
            PruneOrphanDockables(treeRoot, validContentIds);

            if (!ContainsArea(treeRoot, "DocumentArea"))
            {
                return false;
            }

            result = parsed;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool ContainsArea(IDockableTree root, string areaId)
    {
        foreach (var element in root)
        {
            if (element is DockableArea area && area.Id == areaId)
            {
                return true;
            }
        }

        return false;
    }

    private static void ReconcileAreaMetadata(IDockableTree loadedRoot, IDockableTree defaultRoot)
    {
        Dictionary<string, DockableArea> defaultAreas = new();
        Dictionary<string, DockableTree> defaultTrees = new();

        foreach (var element in defaultRoot)
        {
            if (string.IsNullOrEmpty(element.Id))
            {
                continue;
            }

            switch (element)
            {
                case DockableArea area:
                    defaultAreas[area.Id] = area;
                    break;
                case DockableTree tree:
                    defaultTrees[tree.Id] = tree;
                    break;
            }
        }

        foreach (var element in loadedRoot)
        {
            if (string.IsNullOrEmpty(element.Id))
            {
                continue;
            }

            switch (element)
            {
                case DockableArea area when defaultAreas.TryGetValue(area.Id, out var defaultArea):
                    area.FallbackContent = defaultArea.FallbackContent;
                    area.CloseRegionOnEmpty = defaultArea.CloseRegionOnEmpty;
                    break;
                case DockableTree tree when defaultTrees.TryGetValue(tree.Id, out var defaultTree):
                    tree.AutoExpand = defaultTree.AutoExpand;
                    break;
            }
        }
    }

    private static void PruneOrphanDockables(IDockableTree root, HashSet<string> validContentIds)
    {
        List<(IDockableHost Host, IDockable Dockable)> orphans = new();

        foreach (var element in root)
        {
            if (element is not IDockableHost host)
            {
                continue;
            }

            foreach (var dockable in host.Dockables.ToArray())
            {
                if (dockable != null && !validContentIds.Contains(dockable.Id))
                {
                    orphans.Add((host, dockable));
                }
            }
        }

        foreach (var (host, dockable) in orphans)
        {
            host.RemoveDockable(dockable);
        }
    }

    private LayoutTree BuildDefaultLayoutTree()
    {
        return new LayoutTree
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
    }

    private void WindowOnClosed(object? sender, EventArgs e)
    {
        if (sender is HostWindow hostWindow)
        {
            floatedWindows.Remove(hostWindow);
        }
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
                    dockableHost.Context.FocusedTarget = dockableHost;
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
