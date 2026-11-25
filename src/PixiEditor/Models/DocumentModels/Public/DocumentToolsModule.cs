using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Tools;
using PixiEditor.ViewModels.Tools.Tools;

namespace PixiEditor.Models.DocumentModels.Public;

internal class DocumentToolsModule
{
    private IDocument Document { get; set; }
    private DocumentInternalParts Internals { get; set; }

    private Func<UpdateableChangeExecutor>? lastCreateExecutor;

    public DocumentToolsModule(IDocument doc, DocumentInternalParts internals)
    {
        this.Document = doc;
        this.Internals = internals;
    }

    public void UseSymmetry(SymmetryAxisDirection dir) =>
        Internals.ChangeController.TryStartExecutor(new SymmetryExecutor(dir));

    public void UseGuide(Guid id)
    {
        bool force = Internals.ChangeController.GetCurrentExecutorType() == ExecutorType.ToolLinked;
        Internals.ChangeController.TryStartExecutor(new GuideExecutor(id), force);
    }

    public void UseOpacitySlider() => TryStartExecutor<StructureMemberOpacityExecutor>();

    public void UsePenTool() => TryStartExecutor<PenToolExecutor>();

    public void UseEraserTool() => TryStartExecutor<EraserToolExecutor>();

    public void UseColorPickerTool() => TryStartExecutor<ColorPickerToolExecutor>();

    public void UseRasterRectangleTool()
    {
        bool force = Internals.ChangeController.GetCurrentExecutorType() == ExecutorType.ToolLinked;
        TryStartExecutor<RasterRectangleToolExecutor>(force);
    }

    public void UseRasterEllipseTool()
    {
        bool force = Internals.ChangeController.GetCurrentExecutorType() == ExecutorType.ToolLinked;
        TryStartExecutor<RasterEllipseToolExecutor>(force);
    }

    public void UseRasterLineTool()
    {
        bool force = Internals.ChangeController.GetCurrentExecutorType() == ExecutorType.ToolLinked;
        TryStartExecutor<RasterLineToolExecutor>(force);
    }

    public void UseVectorEllipseTool()
    {
        bool force = Internals.ChangeController.GetCurrentExecutorType() == ExecutorType.ToolLinked;
        TryStartExecutor<VectorEllipseToolExecutor>(force);
    }

    public void UseVectorRectangleTool()
    {
        bool force = Internals.ChangeController.GetCurrentExecutorType() == ExecutorType.ToolLinked;
        TryStartExecutor<VectorRectangleToolExecutor>(force);
    }

    public void UseVectorLineTool()
    {
        bool force = Internals.ChangeController.GetCurrentExecutorType() == ExecutorType.ToolLinked;
        TryStartExecutor<VectorLineToolExecutor>(force);
    }

    public void UseVectorPathTool()
    {
        bool force = Internals.ChangeController.GetCurrentExecutorType() == ExecutorType.ToolLinked;
        TryStartExecutor<VectorPathToolExecutor>(force);
    }

    public void UseSelectTool() => TryStartExecutor<SelectToolExecutor>();

    public void UseFloodFillTool() => TryStartExecutor<FloodFillToolExecutor>();

    public void UseLassoTool() => TryStartExecutor<LassoToolExecutor>();

    public void UseMagicWandTool() => TryStartExecutor<MagicWandToolExecutor>();

    public void UseBrushBasedTool(BrushBasedToolViewModel brushBasedToolViewModel)
    {
        lastCreateExecutor = () => new BrushBasedExecutor(brushBasedToolViewModel);
        TryStartExecutor(new BrushBasedExecutor(brushBasedToolViewModel));
    }

    public void UseTextTool()
    {
        bool force = Internals.ChangeController.GetCurrentExecutorType() == ExecutorType.ToolLinked;
        TryStartExecutor<VectorTextToolExecutor>(force);
    }

    public bool TryStopActiveTool()
    {
        return Internals.ChangeController.TryStopActiveExecutor();
    }

    private bool TryStartExecutor<T>(bool force = false) where T : UpdateableChangeExecutor, new()
    {
        lastCreateExecutor = () => new T();
        return Internals.ChangeController.TryStartExecutor<T>(force);
    }

    private bool TryStartExecutor(UpdateableChangeExecutor executor, bool force = false)
    {
        return Internals.ChangeController.TryStartExecutor(executor, force);
    }

    public void UseLastKnownTool()
    {
        if(lastCreateExecutor != null)
            TryStartExecutor(lastCreateExecutor());
    }
}
