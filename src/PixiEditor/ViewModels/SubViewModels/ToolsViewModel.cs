﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Avalonia.Input;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.ChangeableDocument;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Models.Preferences;
using Drawie.Backend.Core.Numerics;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;
using PixiEditor.Models.AnalyticsAPI;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Commands.Attributes.Evaluators;
using PixiEditor.Models.Commands.CommandContext;
using PixiEditor.Models.Config;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Events;
using PixiEditor.Models.Handlers;
using Drawie.Numerics;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.ViewModels.Document;
using PixiEditor.ViewModels.Tools;
using PixiEditor.ViewModels.Tools.Tools;
using PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;

namespace PixiEditor.ViewModels.SubViewModels;
#nullable enable
[Command.Group("PixiEditor.Tools", "TOOLS")]
internal class ToolsViewModel : SubViewModel<ViewModelMain>, IToolsHandler
{
    private RightClickMode rightClickMode = PixiEditorSettings.Tools.RightClickMode.Value;
    public ZoomToolViewModel? ZoomTool => GetTool<ZoomToolViewModel>();

    public IToolHandler? LastActionTool { get; private set; }

    public RightClickMode RightClickMode
    {
        get => rightClickMode;
        set
        {
            if (SetProperty(ref rightClickMode, value))
            {
                PixiEditorSettings.Tools.RightClickMode.Value = value;
            }
        }
    }

    public bool EnableSharedToolbar
    {
        get => PixiEditorSettings.Tools.EnableSharedToolbar.Value;
        set
        {
            if (EnableSharedToolbar == value)
            {
                return;
            }

            PixiEditorSettings.Tools.EnableSharedToolbar.Value = value;
            OnPropertyChanged(nameof(EnableSharedToolbar));
        }
    }

    public bool SelectionTintingEnabled
    {
        get => PixiEditorSettings.Tools.SelectionTintingEnabled.Value;
        set
        {
            if (SelectionTintingEnabled == value)
                return;

            PixiEditorSettings.Tools.SelectionTintingEnabled.Value = value;
            OnPropertyChanged();
        }
    }

    private Cursor? toolCursor;

    public Cursor? ToolCursor
    {
        get => toolCursor;
        set => SetProperty(ref toolCursor, value);
    }

    public IToolSizeToolbar? ActiveBasicToolbar
    {
        get => ActiveTool?.Toolbar as IToolSizeToolbar;
    }

    private IToolHandler? activeTool;

    public IToolHandler? ActiveTool
    {
        get => activeTool;
        private set
        {
            SetProperty(ref activeTool, value);
            OnPropertyChanged(nameof(ActiveBasicToolbar));
        }
    }

    public IToolSetHandler ActiveToolSet
    {
        get => _activeToolSet!;
        private set => SetProperty(ref _activeToolSet, value);
    }

    ICollection<IToolSetHandler> IToolsHandler.AllToolSets => AllToolSets;

    public ObservableCollection<IToolSetHandler> AllToolSets { get; } = new();
    public List<string> AllToolSetNames => AllToolSets.Select(x => x.Name).ToList();
    public List<IToolSetHandler> NonSelectedToolSets => AllToolSets.Where(x => x != ActiveToolSet).ToList();

    public event EventHandler<SelectedToolEventArgs>? SelectedToolChanged;

    private bool shiftIsDown;
    private bool ctrlIsDown;
    private bool altIsDown;
    private Key lastKey;

    private ToolViewModel _preTransientTool;

    private List<IToolHandler> allTools = new();
    private List<ToolSetConfig> originalToolSets = new();
    private IToolSetHandler? _activeToolSet;

    public ToolsViewModel(ViewModelMain owner)
        : base(owner)
    {
        owner.DocumentManagerSubViewModel.ActiveDocumentChanged += ActiveDocumentChanged;
        PixiEditorSettings.Tools.PrimaryToolset.ValueChanged += PrimaryToolsetOnValueChanged;
        SubscribeSettingsValueChanged(PixiEditorSettings.Tools.SelectionTintingEnabled, nameof(SelectionTintingEnabled));
    }

    private void PrimaryToolsetOnValueChanged(Setting<string> setting, string? newPrimaryToolset)
    {
        var toolset = AllToolSets.FirstOrDefault(x => x.Name == newPrimaryToolset);

        if (toolset is not null)
        {
            var orderedToolSetConfig = originalToolSets
                .OrderByDescending(toolSet => toolSet.Name == newPrimaryToolset)
                .ToList();
            var toolsets = new List<IToolSetHandler>(AllToolSets);
            AllToolSets.Clear();

            foreach (var toolSetConfig in orderedToolSetConfig)
            {
                var foundToolSet = toolsets.FirstOrDefault(x => x.Name == toolSetConfig.Name);
                if (foundToolSet is not null)
                {
                    AllToolSets.Add(foundToolSet);
                }
            }

            SetActiveToolSet(toolset);
        }
    }

    public void SetupTools(IServiceProvider services, ToolSetsConfig toolSetConfig)
    {
        allTools = services.GetServices<IToolHandler>().ToList();

        ToolSetConfig activeToolSetConfig = toolSetConfig.FirstOrDefault();

        if (activeToolSetConfig is null)
        {
            throw new InvalidOperationException("No tool set configuration found.");
        }

        AllToolSets.Clear();
        AddToolSets(toolSetConfig);
        SetActiveToolSet(AllToolSets.First());
    }

    [Command.Internal("PixiEditor.Tools.SetActiveToolSet", AnalyticsTrack = true)]
    public void SetActiveToolSet(IToolSetHandler toolSetHandler)
    {
        ActiveTool?.OnToolDeselected(false);
        ActiveToolSet = toolSetHandler;
        ActiveToolSet.ApplyToolSetSettings();
        UpdateEnabledState();

        ActiveTool?.OnToolSelected(false);

        OnPropertyChanged(nameof(NonSelectedToolSets));
    }

    [Command.Basic("PixiEditor.Tools.ToggleSelectionTinting", "TOGGLE_TINTING_SELECTION", "TOGGLE_TINTING_SELECTION_DESCRIPTIVE", AnalyticsTrack = true)]
    public void ToggleTintSelection() => SelectionTintingEnabled = !SelectionTintingEnabled;

    public void SetupToolsTooltipShortcuts()
    {
        foreach (IToolHandler tool in allTools)
        {
            if (tool is ToolViewModel toolVm)
            {
                var combination = Owner.ShortcutController.GetToolShortcut(tool.GetType());
                if (combination is not null)
                {
                    toolVm.Shortcut = combination.Value;
                }
            }
        }
    }

    public T? GetTool<T>()
        where T : IToolHandler
    {
        return (T?)ActiveToolSet?.Tools.Where(static tool => tool is T).FirstOrDefault();
    }

    public void SetActiveTool<T>(bool transient)
        where T : IToolHandler
    {
        SetActiveTool(typeof(T), transient, null);
    }

    public void SetActiveTool(Type toolType, bool transient) => SetActiveTool(toolType, transient, null);

    [Command.Basic("PixiEditor.Tools.ApplyTransform", "APPLY_TRANSFORM", "", Key = Key.Enter, AnalyticsTrack = true)]
    public void ApplyTransform()
    {
        DocumentViewModel? doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;
        doc.EventInlet.OnApplyTransform();
    }

    [Command.Internal("PixiEditor.Tools.SwitchToolSet", AnalyticsTrack = true,
        CanExecute = "PixiEditor.HasNextToolSet")]
    [Command.Basic("PixiEditor.Tools.NextToolSet", true, "NEXT_TOOL_SET", "NEXT_TOOL_SET",
        Modifiers = KeyModifiers.Shift,
        Key = Key.E, AnalyticsTrack = true)]
    [Command.Basic("PixiEditor.Tools.PreviousToolSet", false, "PREVIOUS_TOOL_SET", "PREVIOUS_TOOL_SET",
        Modifiers = KeyModifiers.Shift,
        Key = Key.Q, AnalyticsTrack = true)]
    public void SwitchToolSet(bool forward)
    {
        int currentIndex = AllToolSets.IndexOf(ActiveToolSet);
        int nextIndex = currentIndex + (forward ? 1 : -1);
        if (nextIndex >= AllToolSets.Count)
        {
            nextIndex = 0;
        }

        if (nextIndex < 0)
        {
            nextIndex = AllToolSets.Count - 1;
        }

        SetActiveToolSet(AllToolSets.ElementAt(nextIndex));
    }

    [Evaluator.CanExecute("PixiEditor.HasNextToolSet",
        nameof(ActiveToolSet),
        nameof(AllToolSets))]
    public bool HasNextToolSet(bool next)
    {
        int currentIndex = AllToolSets.IndexOf(ActiveToolSet);
        int nextIndex = currentIndex + (next ? 1 : -1);
        if (nextIndex < 0 || nextIndex >= AllToolSets.Count)
        {
            return false;
        }

        return AllToolSets.ElementAt(nextIndex) != ActiveToolSet;
    }

    [Command.Internal("PixiEditor.Tools.SelectTool", CanExecute = "PixiEditor.HasDocument")]
    public void SetActiveTool(ToolViewModel tool)
    {
        SetActiveTool(tool, false, null);
    }

    public void SetActiveTool(IToolHandler tool, bool transient) => SetActiveTool(tool, transient, null);

    public void SetActiveTool(IToolHandler tool, bool transient, ICommandExecutionSourceInfo? sourceInfo)
    {
        if (Owner.DocumentManagerSubViewModel.ActiveDocument is { PointerDragChangeInProgress: true }) return;

        if (ActiveTool == tool)
        {
            ActiveTool.IsTransient = transient;
            LastActionTool = ActiveTool;
            return;
        }

        if (ActiveTool != null)
        {
            ActiveTool.OnToolDeselected(transient);
            ActiveTool.Toolbar.SettingChanged -= ToolbarSettingChanged;
        }

        bool wasTransient = ActiveTool?.IsTransient ?? false;
        if (ActiveTool != null) ActiveTool.IsTransient = false;
        bool shareToolbar = EnableSharedToolbar;
        if (ActiveTool is not null)
        {
            ActiveTool.IsActive = false;
            if (shareToolbar)
                ActiveTool.Toolbar.SaveToolbarSettings();
        }

        LastActionTool = ActiveTool;
        ActiveTool = tool;

        ActiveTool.Toolbar.SettingChanged += ToolbarSettingChanged;

        if (shareToolbar)
        {
            ActiveTool.Toolbar.LoadSharedSettings();
        }

        if (LastActionTool != ActiveTool)
        {
            SelectedToolChanged?.Invoke(this, new SelectedToolEventArgs(LastActionTool, ActiveTool));
        }

        //update old tool
        LastActionTool?.KeyChanged(false, false, false, Key.None);
        //update new tool
        ActiveTool.KeyChanged(ctrlIsDown, shiftIsDown, altIsDown, lastKey);
        ActiveTool.OnToolSelected(wasTransient);

        tool.IsActive = true;
        ActiveTool.IsTransient = transient;
        SetToolCursor(tool.GetType());

        if (Owner.StylusSubViewModel != null)
        {
            Owner.StylusSubViewModel.ToolSetByStylus = false;
        }

        if (ActiveTool != null || LastActionTool != null)
        {
            Analytics.SendSwitchToTool(tool, LastActionTool, sourceInfo);
        }
    }

    public void SetTool(object parameter)
    {
        ICommandExecutionSourceInfo source = null;

        if (parameter is CommandExecutionContext context)
        {
            source = context.SourceInfo;
            parameter = context.Parameter;
        }

        if (parameter is Type type)
        {
            SetActiveTool(type, false, source);
            return;
        }

        ToolViewModel tool = (ToolViewModel)parameter;
        SetActiveTool(tool.GetType(), false, source);
    }

    [Command.Basic("PixiEditor.Tools.IncreaseSize", 1d, "INCREASE_TOOL_SIZE", "INCREASE_TOOL_SIZE",
        CanExecute = "PixiEditor.Tools.CanChangeToolSize", Key = Key.OemCloseBrackets, AnalyticsTrack = true)]
    [Command.Basic("PixiEditor.Tools.DecreaseSize", -1d, "DECREASE_TOOL_SIZE", "DECREASE_TOOL_SIZE",
        CanExecute = "PixiEditor.Tools.CanChangeToolSize", Key = Key.OemOpenBrackets, AnalyticsTrack = true)]
    public void ChangeToolSize(double increment)
    {
        if (ActiveTool?.Toolbar is not IToolSizeToolbar toolbar || !CanChangeToolSize())
            return;
        double newSize = toolbar.ToolSize + increment;
        if (newSize > 0)
            toolbar.ToolSize = newSize;
    }

    [Evaluator.CanExecute("PixiEditor.Tools.CanChangeToolSize",
        nameof(ActiveTool))]
    public bool CanChangeToolSize() => Owner.ToolsSubViewModel.ActiveTool?.Toolbar is IToolSizeToolbar
                                       && Owner.ToolsSubViewModel.ActiveTool is not PenToolViewModel
                                       {
                                           PixelPerfectEnabled: true
                                       };

    public void SetActiveTool(Type toolType, bool transient, ICommandExecutionSourceInfo sourceInfo)
    {
        if (!typeof(ToolViewModel).IsAssignableFrom(toolType))
            throw new ArgumentException($"'{toolType}' does not inherit from {typeof(ToolViewModel)}");
        IToolHandler foundTool = ActiveToolSet!.Tools.FirstOrDefault(x => x.GetType().IsAssignableFrom(toolType));
        if (foundTool == null)
        {
            foundTool = allTools.FirstOrDefault(x => x.GetType().IsAssignableFrom(toolType));
            if(foundTool == null)
                return;

            var toolset = AllToolSets.FirstOrDefault(x => x.Tools.Contains(foundTool));
            if (toolset is not null)
            {
                SetActiveToolSet(toolset);
            }
        }

        SetActiveTool(foundTool, transient, sourceInfo);
    }

    public void RestorePreviousTool()
    {
        if (LastActionTool != null)
        {
            SetActiveTool(LastActionTool, false);
        }
        else
        {
            SetActiveTool<PenToolViewModel>(false);
        }
    }

    private void SetToolCursor(Type tool)
    {
        if (tool is not null)
        {
            ToolCursor = ActiveTool?.Cursor;
        }
        else
        {
            ToolCursor = new Cursor(StandardCursorType.Arrow);
        }
    }

    public void HandleToolRepeatShortcutDown()
    {
        if (ActiveTool == null) return;
        if (ActiveTool is null or { IsTransient: false })
        {
            ShortcutController.BlockShortcutExecution("ShortcutDown");
            ActiveTool.IsTransient = true;
        }
    }

    public void HandleToolShortcutUp()
    {
        if (ActiveTool == null) return;
        if (ActiveTool.IsTransient && LastActionTool is { } tool)
            SetActiveTool(tool, false);
    }

    public void UseToolEventInlet(VecD canvasPos, MouseButton button)
    {
        ActiveTool.UsedWith = button;
        if (ActiveTool.StopsLinkedToolOnUse)
        {
            ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument?.Operations.TryStopToolLinkedExecutor();
        }

        bool waitForChange = false;

        if (ActiveTool is not { CanBeUsedOnActiveLayer: true })
        {
            if (ActiveTool.LayerTypeToCreateOnEmptyUse == null) return;

            using var changeBlock = Owner.DocumentManagerSubViewModel.ActiveDocument.Operations.StartChangeBlock();
            Guid? createdLayer = Owner.LayersSubViewModel.NewLayer(
                ActiveTool.LayerTypeToCreateOnEmptyUse,
                ActionSource.Automated,
                ActiveTool.DefaultNewLayerName);
            if (createdLayer is not null)
            {
                Owner.DocumentManagerSubViewModel.ActiveDocument.Operations.SetSelectedMember(createdLayer.Value);
            }

            changeBlock.ExecuteQueuedActions();
        }

        ActiveTool.UseTool(canvasPos);
    }

    public void ConvertedKeyDownInlet(FilteredKeyEventArgs args)
    {
        ActiveTool?.KeyChanged(args.IsCtrlDown, args.IsShiftDown, args.IsAltDown, args.Key);
    }

    public void ConvertedKeyUpInlet(FilteredKeyEventArgs args)
    {
        ActiveTool?.KeyChanged(args.IsCtrlDown, args.IsShiftDown, args.IsAltDown, args.Key);
    }

    public void OnPostUndoInlet()
    {
        ActiveTool?.OnPostUndoInlet();
    }

    public void OnPostRedoInlet()
    {
        ActiveTool?.OnPostRedoInlet();
    }

    public void OnPreUndoInlet()
    {
        ActiveTool?.OnPreUndoInlet();
    }

    public void QuickToolSwitchInlet()
    {
        var document = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (document is null)
            return;

        document.EventInlet.QuickToolSwitchInlet();
    }

    public void OnPreRedoInlet()
    {
        ActiveTool?.OnPreRedoInlet();
    }

    private void ToolbarSettingChanged(string settingName, object value)
    {
        var document = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (document is null)
            return;

        document.EventInlet.SettingsChanged(settingName, value);
    }

    private void AddToolSets(ToolSetsConfig toolSetConfig)
    {
        var primaryToolSet = PixiEditorSettings.Tools.PrimaryToolset.Value;
        if (string.IsNullOrEmpty(primaryToolSet))
        {
            primaryToolSet = toolSetConfig.First().Name;
        }

        originalToolSets = toolSetConfig.ToList();

        var orderedToolSetConfig = toolSetConfig
            .OrderByDescending(toolSet => toolSet.Name == primaryToolSet)
            .ToList();

        foreach (ToolSetConfig toolSet in orderedToolSetConfig)
        {
            var toolSetViewModel = new ToolSetViewModel(toolSet.Name, toolSet.Icon);

            foreach (var toolFromToolset in toolSet.Tools)
            {
                IToolHandler? tool = allTools.FirstOrDefault(tool => tool.ToolName == toolFromToolset.ToolName);
                tool.SetToolSetSettings(toolSetViewModel, toolFromToolset.Settings);

                if (!string.IsNullOrEmpty(toolFromToolset.Icon))
                {
                    toolSetViewModel.IconOverwrites[tool] = PixiPerfectIconExtensions.TryGetByName(toolFromToolset.Icon) ?? PixiPerfectIcons.Placeholder;
                }

                if (tool is null)
                {
#if DEBUG
                    throw new InvalidOperationException($"Tool '{tool}' not found.");
#endif

                    continue;
                }

                toolSetViewModel.AddTool(tool);
            }

            AllToolSets.Add(toolSetViewModel);
        }
    }

    private void ActiveDocumentChanged(object? sender, DocumentChangedEventArgs e)
    {
        if (e.OldDocument is not null)
        {
            e.OldDocument.PropertyChanged -= DocumentOnPropertyChanged;
            e.OldDocument.LayersChanged -= DocumentOnLayersChanged;
            e.OldDocument.AnimationDataViewModel.ActiveFrameChanged -= ActiveFrameChanged;
        }

        if (e.NewDocument is not null)
        {
            e.NewDocument.PropertyChanged += DocumentOnPropertyChanged;
            e.NewDocument.LayersChanged += DocumentOnLayersChanged;
            e.NewDocument.AnimationDataViewModel.ActiveFrameChanged += ActiveFrameChanged;
            UpdateEnabledState();
        }
    }

    private void ActiveFrameChanged(int oldFrame, int newFrame)
    {
        UpdateActiveFrame(newFrame);
    }

    private void DocumentOnLayersChanged(object? sender, LayersChangedEventArgs e)
    {
        UpdateEnabledState();
    }

    private void DocumentOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(DocumentViewModel.SelectedStructureMember)
            or nameof(DocumentViewModel.SoftSelectedStructureMembers))
        {
            UpdateEnabledState();
        }
    }

    private void UpdateActiveFrame(int newFrame)
    {
        ActiveTool?.OnActiveFrameChanged(newFrame);
    }

    private void UpdateEnabledState()
    {
        var doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;

        foreach (var toolHandler in ActiveToolSet.Tools)
        {
            if (toolHandler is ToolViewModel tool)
            {
                List<IStructureMemberHandler> selectedLayers = new List<IStructureMemberHandler>
                {
                    doc.SelectedStructureMember
                };

                selectedLayers.AddRange(doc.SoftSelectedStructureMembers.Except(selectedLayers));
                tool.SelectedLayersChanged(selectedLayers.ToArray());
            }
        }
    }
}
