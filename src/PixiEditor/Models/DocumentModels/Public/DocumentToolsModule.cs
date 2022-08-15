using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace Models.DocumentModels.Public;
internal class DocumentToolsModule
{
    private DocumentViewModel Document { get; set; }
    private DocumentInternalParts Internals { get; set; }

    public DocumentToolsModule(DocumentViewModel doc, DocumentInternalParts internals)
    {
        this.Document = doc;
        this.Internals = internals;
    }

    public void UseOpacitySlider() => Internals.ChangeController.TryStartExecutor<StructureMemberOpacityExecutor>();

    public void UseShiftLayerTool() => Internals.ChangeController.TryStartExecutor<ShiftLayerExecutor>();

    public void UsePenTool() => Internals.ChangeController.TryStartExecutor<PenToolExecutor>();

    public void UseEraserTool() => Internals.ChangeController.TryStartExecutor<EraserToolExecutor>();

    public void UseColorPickerTool() => Internals.ChangeController.TryStartExecutor<ColorPickerToolExecutor>();

    public void UseRectangleTool()
    {
        bool force = Internals.ChangeController.GetCurrentExecutorType() == ExecutorType.ToolLinked;
        Internals.ChangeController.TryStartExecutor<RectangleToolExecutor>(force);
    }

    public void UseEllipseTool()
    {
        bool force = Internals.ChangeController.GetCurrentExecutorType() == ExecutorType.ToolLinked;
        Internals.ChangeController.TryStartExecutor<EllipseToolExecutor>(force);
    }

    public void UseLineTool()
    {
        bool force = Internals.ChangeController.GetCurrentExecutorType() == ExecutorType.ToolLinked;
        Internals.ChangeController.TryStartExecutor<LineToolExecutor>(force);
    }

    public void UseSelectTool() => Internals.ChangeController.TryStartExecutor<SelectToolExecutor>();

    public void UseBrightnessTool() => Internals.ChangeController.TryStartExecutor<BrightnessToolExecutor>();

    public void UseFloodFillTool() => Internals.ChangeController.TryStartExecutor<FloodFillToolExecutor>();
}
