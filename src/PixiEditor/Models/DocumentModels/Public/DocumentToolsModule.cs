using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Tools;

namespace PixiEditor.Models.DocumentModels.Public;

internal class DocumentToolsModule
{
    private IDocument Document { get; set; }
    private DocumentInternalParts Internals { get; set; }

    public DocumentToolsModule(IDocument doc, DocumentInternalParts internals)
    {
        this.Document = doc;
        this.Internals = internals;
    }

    public void UseSymmetry(SymmetryAxisDirection dir) =>
        Internals.ChangeController.TryStartExecutor(new SymmetryExecutor(dir));

    public void UseOpacitySlider() => Internals.ChangeController.TryStartExecutor<StructureMemberOpacityExecutor>();


    public void UsePenTool() => Internals.ChangeController.TryStartExecutor<PenToolExecutor>();

    public void UseEraserTool() => Internals.ChangeController.TryStartExecutor<EraserToolExecutor>();

    public void UseColorPickerTool() => Internals.ChangeController.TryStartExecutor<ColorPickerToolExecutor>();

    public void UseRasterRectangleTool()
    {
        bool force = Internals.ChangeController.GetCurrentExecutorType() == ExecutorType.ToolLinked;
        Internals.ChangeController.TryStartExecutor<RasterRectangleToolExecutor>(force);
    }

    public void UseRasterEllipseTool()
    {
        bool force = Internals.ChangeController.GetCurrentExecutorType() == ExecutorType.ToolLinked;
        Internals.ChangeController.TryStartExecutor<RasterEllipseToolExecutor>(force);
    }

    public void UseRasterLineTool()
    {
        bool force = Internals.ChangeController.GetCurrentExecutorType() == ExecutorType.ToolLinked;
        Internals.ChangeController.TryStartExecutor<RasterLineToolExecutor>(force);
    }

    public void UseVectorEllipseTool()
    {
        bool force = Internals.ChangeController.GetCurrentExecutorType() == ExecutorType.ToolLinked;
        Internals.ChangeController.TryStartExecutor<VectorEllipseToolExecutor>(force);
    }

    public void UseVectorRectangleTool()
    {
        bool force = Internals.ChangeController.GetCurrentExecutorType() == ExecutorType.ToolLinked;
        Internals.ChangeController.TryStartExecutor<VectorRectangleToolExecutor>(force);
    }

    public void UseVectorLineTool()
    {
        bool force = Internals.ChangeController.GetCurrentExecutorType() == ExecutorType.ToolLinked;
        Internals.ChangeController.TryStartExecutor<VectorLineToolExecutor>(force);
    }

    public void UseVectorPathTool()
    {
        bool force = Internals.ChangeController.GetCurrentExecutorType() == ExecutorType.ToolLinked;
        Internals.ChangeController.TryStartExecutor<VectorPathToolExecutor>(force);
    }

    public void UseSelectTool() => Internals.ChangeController.TryStartExecutor<SelectToolExecutor>();

    public void UseBrightnessTool() => Internals.ChangeController.TryStartExecutor<BrightnessToolExecutor>();

    public void UseFloodFillTool() => Internals.ChangeController.TryStartExecutor<FloodFillToolExecutor>();

    public void UseLassoTool() => Internals.ChangeController.TryStartExecutor<LassoToolExecutor>();

    public void UseMagicWandTool() => Internals.ChangeController.TryStartExecutor<MagicWandToolExecutor>();

    public void UseTextTool()
    {
        bool force = Internals.ChangeController.GetCurrentExecutorType() == ExecutorType.ToolLinked;
        Internals.ChangeController.TryStartExecutor<VectorTextToolExecutor>(force);
    }
}
