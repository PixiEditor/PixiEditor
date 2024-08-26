using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Input;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Models.Preferences;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;
using PixiEditor.Models.AnalyticsAPI;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Commands.Attributes.Evaluators;
using PixiEditor.Models.Commands.CommandContext;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Events;
using PixiEditor.Models.Handlers;
using PixiEditor.Numerics;
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

    private Cursor? toolCursor;
    public Cursor? ToolCursor
    {
        get => toolCursor;
        set => SetProperty(ref toolCursor, value);
    }

    public BasicToolbar? ActiveBasicToolbar
    {
        get => ActiveTool?.Toolbar as BasicToolbar;
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

    ICollection<IToolHandler> IToolsHandler.ToolSet => ToolSet;
    public ToolSetViewModel ToolSet { get; private set; }

    public event EventHandler<SelectedToolEventArgs>? SelectedToolChanged;

    private bool shiftIsDown;
    private bool ctrlIsDown;
    private bool altIsDown;

    private ToolViewModel _preTransientTool;


    public ToolsViewModel(ViewModelMain owner)
        : base(owner)
    {
    }

    public void SetupTools(IServiceProvider services)
    {
        ToolSet = new ObservableCollection<IToolHandler>(services.GetServices<IToolHandler>());
    }

    public void SetupToolsTooltipShortcuts(IServiceProvider services)
    {
        foreach (ToolViewModel tool in ToolSet!)
        {
            tool.Shortcut = Owner.ShortcutController.GetToolShortcut(tool.GetType());
        }
    }

    public T? GetTool<T>()
        where T : IToolHandler
    {
        return (T?)ToolSet?.Where(static tool => tool is T).FirstOrDefault();
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

    [Command.Internal("PixiEditor.Tools.SelectTool", CanExecute = "PixiEditor.HasDocument")]
    public void SetActiveTool(ToolViewModel tool)
    {
        SetActiveTool(tool, false, null);
    }

    public void SetActiveTool(IToolHandler tool, bool transient) => SetActiveTool(tool, transient, null);

    public void SetActiveTool(IToolHandler tool, bool transient, ICommandExecutionSourceInfo? sourceInfo)
    {
        if(Owner.DocumentManagerSubViewModel.ActiveDocument is { PointerDragChangeInProgress: true }) return;

        if (ActiveTool == tool)
        {
            ActiveTool.IsTransient = transient;
            return;
        }

        ActiveTool?.OnDeselecting();

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

        if (shareToolbar)
        {
            ActiveTool.Toolbar.LoadSharedSettings();
        }

        if (LastActionTool != ActiveTool)
            SelectedToolChanged?.Invoke(this, new SelectedToolEventArgs(LastActionTool, ActiveTool));

        //update old tool
        LastActionTool?.ModifierKeyChanged(false, false, false);
        //update new tool
        ActiveTool.ModifierKeyChanged(ctrlIsDown, shiftIsDown, altIsDown);
        ActiveTool.OnSelected();

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

    [Command.Basic("PixiEditor.Tools.IncreaseSize", 1, "INCREASE_TOOL_SIZE", "INCREASE_TOOL_SIZE", CanExecute = "PixiEditor.Tools.CanChangeToolSize", Key = Key.OemCloseBrackets, AnalyticsTrack = true)]
    [Command.Basic("PixiEditor.Tools.DecreaseSize", -1, "DECREASE_TOOL_SIZE", "DECREASE_TOOL_SIZE", CanExecute = "PixiEditor.Tools.CanChangeToolSize", Key = Key.OemOpenBrackets, AnalyticsTrack = true)]
    public void ChangeToolSize(int increment)
    {
        if (ActiveTool?.Toolbar is not BasicToolbar toolbar)
            return;
        int newSize = toolbar.ToolSize + increment;
        if (newSize > 0)
            toolbar.ToolSize = newSize;
    }

    [Evaluator.CanExecute("PixiEditor.Tools.CanChangeToolSize")]
    public bool CanChangeToolSize() => Owner.ToolsSubViewModel.ActiveTool?.Toolbar is BasicToolbar
                                       && Owner.ToolsSubViewModel.ActiveTool is not PenToolViewModel
                                       {
                                           PixelPerfectEnabled: true
                                       };

    public void SetActiveTool(Type toolType, bool transient, ICommandExecutionSourceInfo sourceInfo)
    {
        if (!typeof(ToolViewModel).IsAssignableFrom(toolType))
            throw new ArgumentException($"'{toolType}' does not inherit from {typeof(ToolViewModel)}");
        IToolHandler foundTool = ToolSet!.First(x => x.GetType().IsAssignableFrom(toolType));
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
        if(ActiveTool == null) return;
        if (ActiveTool is null or { IsTransient: false })
        {
            ShortcutController.BlockShortcutExecution("ShortcutDown");
            ActiveTool.IsTransient = true;
        }
    }
    
    public void HandleToolShortcutUp()
    {
        if(ActiveTool == null) return;
        if (ActiveTool.IsTransient && LastActionTool is { } tool)
            SetActiveTool(tool, false);
        ShortcutController.UnblockShortcutExecution("ShortcutDown");
    }

    public void UseToolEventInlet(VecD canvasPos, MouseButton button)
    {
        if (ActiveTool == null) return;

        ActiveTool.UsedWith = button;
        if (ActiveTool.StopsLinkedToolOnUse)
        {
            ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument?.Operations.TryStopToolLinkedExecutor();
        }

        ActiveTool.UseTool(canvasPos);
    }

    public void ConvertedKeyDownInlet(FilteredKeyEventArgs args)
    {
        ActiveTool?.ModifierKeyChanged(args.IsCtrlDown, args.IsShiftDown, args.IsAltDown);
    }

    public void ConvertedKeyUpInlet(FilteredKeyEventArgs args)
    {
        ActiveTool?.ModifierKeyChanged(args.IsCtrlDown, args.IsShiftDown, args.IsAltDown);
    }
}
