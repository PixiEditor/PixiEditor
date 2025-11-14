using Avalonia.Input;
using Drawie.Backend.Core.Vector;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.Tools;

internal abstract class ShapeTool : ToolViewModel, IShapeToolHandler
{

    public override bool UsesColor => true;

    public override bool IsErasable => true;
    public bool DrawEven { get; protected set; }
    public bool DrawFromCenter { get; protected set; }

    public ShapeTool()
    {
        Cursor = new Cursor(StandardCursorType.Cross);
        Toolbar = new FillableShapeToolbar();
    }

    protected override void OnDeselecting(bool transient)
    {
        if (!transient)
        {
            ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument?.Operations.TryStopToolLinkedExecutor();
        }
    }
}
