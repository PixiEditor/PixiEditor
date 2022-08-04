using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
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

    public void UseOpacitySlider() => Internals.ChangeController.TryStartUpdateableChange<StructureMemberOpacityExecutor>();

    public void UsePenTool() => Internals.ChangeController.TryStartUpdateableChange<PenToolExecutor>();

    public void UseEraserTool() => Internals.ChangeController.TryStartUpdateableChange<EraserToolExecutor>();

    public void UseColorPickerTool() => Internals.ChangeController.TryStartUpdateableChange<ColorPickerToolExecutor>();

    public void UseRectangleTool() => Internals.ChangeController.TryStartUpdateableChange<RectangleToolExecutor>();

    public void UseEllipseTool() => Internals.ChangeController.TryStartUpdateableChange<EllipseToolExecutor>();

    public void UseLineTool() => Internals.ChangeController.TryStartUpdateableChange<LineToolExecutor>();

    public void UseSelectTool() => Internals.ChangeController.TryStartUpdateableChange<SelectToolExecutor>();

    public void UseBrightnessTool() => Internals.ChangeController.TryStartUpdateableChange<BrightnessToolExecutor>();
}
