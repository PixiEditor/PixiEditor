using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Events;
using PixiEditor.Models.UserPreferences;
using PixiEditor.ViewModels.SubViewModels.Document;
using PixiEditor.ViewModels.SubViewModels.Tools;
using PixiEditor.ViewModels.SubViewModels.Tools.Tools;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

namespace PixiEditor.ViewModels.SubViewModels.Main;

[Command.Group("PixiEditor.Tools", "Tools")]
internal class ToolsViewModel : SubViewModel<ViewModelMain>
{
    public ZoomToolViewModel ZoomTool => (ZoomToolViewModel)GetTool<ZoomToolViewModel>();

    public ToolViewModel LastActionTool { get; private set; }

    public bool ActiveToolIsTransient { get; set; }

    private Cursor toolCursor;
    public Cursor ToolCursor
    {
        get => toolCursor;
        set => SetProperty(ref toolCursor, value);
    }

    private ToolViewModel activeTool;
    public ToolViewModel ActiveTool
    {
        get => activeTool;
        private set => SetProperty(ref activeTool, value);
    }

    public List<ToolViewModel> ToolSet { get; private set; }

    public event EventHandler<SelectedToolEventArgs> SelectedToolChanged;

    private bool shiftIsDown;
    private bool ctrlIsDown;
    private bool altIsDown;

    public ToolsViewModel(ViewModelMain owner)
        : base(owner)
    { }

    public void SetupTools(IServiceProvider services)
    {
        ToolSet = services.GetServices<ToolViewModel>().ToList();
        SetActiveTool<PenToolViewModel>();
    }

    public void SetupToolsTooltipShortcuts(IServiceProvider services)
    {
        foreach (ToolViewModel tool in ToolSet)
        {
            tool.Shortcut = Owner.ShortcutController.GetToolShortcut(tool.GetType());
        }
    }

    public ToolViewModel? GetTool<T>()
        where T : ToolViewModel
    {
        return ToolSet?.Where(static tool => tool is T).FirstOrDefault();
    }

    public void SetActiveTool<T>()
        where T : ToolViewModel
    {
        SetActiveTool(typeof(T));
    }

    [Command.Basic("PixiEditor.Tools.ApplyTransform", "Apply transform", "", Key = Key.Enter)]
    public void ApplyTransform()
    {
        DocumentViewModel doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;
        doc.OnApplyTransform();
    }

    [Command.Internal("PixiEditor.Tools.SelectTool", CanExecute = "PixiEditor.HasDocument")]
    public void SetActiveTool(ToolViewModel tool)
    {
        if (ActiveTool == tool) return;

        if (!tool.Toolbar.SettingsGenerated)
            tool.Toolbar.GenerateSettings();

        ActiveToolIsTransient = false;
        bool shareToolbar = IPreferences.Current.GetPreference<bool>("EnableSharedToolbar");
        if (ActiveTool != null)
        {
            activeTool.IsActive = false;
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
        LastActionTool?.UpdateActionDisplay(false, false, false);
        //update new tool
        ActiveTool.UpdateActionDisplay(ctrlIsDown, shiftIsDown, altIsDown);

        tool.IsActive = true;
        SetToolCursor(tool.GetType());

        if (Owner.StylusSubViewModel != null)
        {
            Owner.StylusSubViewModel.ToolSetByStylus = false;
        }
    }

    public void SetTool(object parameter)
    {
        if (parameter is Type type)
        {
            SetActiveTool(type);
            return;
        }

        ToolViewModel tool = (ToolViewModel)parameter;
        SetActiveTool(tool.GetType());
    }

    [Command.Basic("PixiEditor.Tools.IncreaseSize", 1, "Increase Tool Size", "Increase Tool Size", Key = Key.OemCloseBrackets)]
    [Command.Basic("PixiEditor.Tools.DecreaseSize", -1, "Decrease Tool Size", "Decrease Tool Size", Key = Key.OemOpenBrackets)]
    public void ChangeToolSize(int increment)
    {
        if (ActiveTool?.Toolbar is not BasicToolbar toolbar)
            return;
        int newSize = toolbar.ToolSize + increment;
        if (newSize > 0)
            toolbar.ToolSize = newSize;
    }

    public void SetActiveTool(Type toolType)
    {
        if (!typeof(ToolViewModel).IsAssignableFrom(toolType)) { throw new ArgumentException($"'{toolType}' does not inherit from {typeof(ToolViewModel)}"); }
        ToolViewModel foundTool = ToolSet.First(x => x.GetType() == toolType);
        SetActiveTool(foundTool);
    }

    private void SetToolCursor(Type tool)
    {
        if (tool != null)
        {
            ToolCursor = ActiveTool.Cursor;
        }
        else
        {
            ToolCursor = Cursors.Arrow;
        }
    }

    public void OnLeftMouseButtonDown(VecD canvasPos)
    {
        ActiveTool?.OnLeftMouseButtonDown(canvasPos);
    }

    public void OnKeyDown(Key key)
    {
        bool shiftIsDown = key is Key.LeftShift or Key.RightShift;
        bool ctrlIsDown = key is Key.LeftCtrl or Key.RightCtrl;
        bool altIsDown = key is Key.LeftAlt or Key.RightAlt;
        if (!shiftIsDown && !ctrlIsDown && !altIsDown)
            return;
        this.shiftIsDown |= shiftIsDown;
        this.ctrlIsDown |= ctrlIsDown;
        this.altIsDown |= altIsDown;

        ActiveTool.UpdateActionDisplay(this.ctrlIsDown, this.shiftIsDown, this.altIsDown);
    }

    public void OnKeyUp(Key key)
    {
        bool shiftIsUp = key is Key.LeftShift or Key.RightShift;
        bool ctrlIsUp = key is Key.LeftCtrl or Key.RightCtrl;
        bool altIsUp = key is Key.LeftAlt or Key.RightAlt;
        if (!shiftIsUp && !ctrlIsUp && !altIsUp)
            return;
        if (shiftIsUp)
            this.shiftIsDown = false;
        if (ctrlIsUp)
            this.ctrlIsDown = false;
        if (altIsUp)
            this.altIsDown = false;
        ActiveTool.UpdateActionDisplay(ctrlIsDown, shiftIsDown, altIsDown);
    }
}
