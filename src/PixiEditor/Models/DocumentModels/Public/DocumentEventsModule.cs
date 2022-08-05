using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.Models.DocumentModels.Public;
internal class DocumentEventsModule
{
    private DocumentViewModel Document { get; }
    private DocumentInternalParts Internals { get; }

    public DocumentEventsModule(DocumentViewModel document, DocumentInternalParts internals)
    {
        Document = document;
        Internals = internals;
    }

    public void OnKeyDown(Key args) { }
    public void OnKeyUp(Key args) { }

    public void OnConvertedKeyDown(Key args) => Internals.ChangeController.ConvertedKeyDownInlet(args);
    public void OnConvertedKeyUp(Key args) => Internals.ChangeController.ConvertedKeyUpInlet(args);

    public void OnCanvasLeftMouseButtonDown(VecD pos) => Internals.ChangeController.LeftMouseButtonDownInlet(pos);
    public void OnCanvasMouseMove(VecD newPos)
    {
        Document.CoordinatesString = $"X: {(int)newPos.X} Y: {(int)newPos.Y}";
        Internals.ChangeController.MouseMoveInlet(newPos);
    }
    public void OnCanvasLeftMouseButtonUp() => Internals.ChangeController.LeftMouseButtonUpInlet();
    public void OnOpacitySliderDragStarted() => Internals.ChangeController.OpacitySliderDragStartedInlet();
    public void OnOpacitySliderDragged(float newValue) => Internals.ChangeController.OpacitySliderDraggedInlet(newValue);
    public void OnOpacitySliderDragEnded() => Internals.ChangeController.OpacitySliderDragEndedInlet();
    public void OnApplyTransform() => Internals.ChangeController.TransformAppliedInlet();
}
