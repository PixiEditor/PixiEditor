using Avalonia.Input;
using PixiEditor.AvaloniaUI.Models.Events;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.AvaloniaUI.Models.Position;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.AvaloniaUI.Models.DocumentModels.Public;
internal class DocumentEventsModule
{
    private IDocument DocumentsHandler { get; }
    private DocumentInternalParts Internals { get; }

    public DocumentEventsModule(IDocument documentsHandler, DocumentInternalParts internals)
    {
        DocumentsHandler = documentsHandler;
        Internals = internals;
    }

    public void OnKeyDown(Key args) { }
    public void OnKeyUp(Key args) { }

    public void OnConvertedKeyDown(FilteredKeyEventArgs args)
    {
        Internals.ChangeController.ConvertedKeyDownInlet(args.Key);
        DocumentsHandler.TransformHandler.KeyModifiersInlet(args.IsShiftDown, args.IsCtrlDown, args.IsAltDown);
    }
    public void OnConvertedKeyUp(FilteredKeyEventArgs args)
    {
        Internals.ChangeController.ConvertedKeyUpInlet(args.Key);
        DocumentsHandler.TransformHandler.KeyModifiersInlet(args.IsShiftDown, args.IsCtrlDown, args.IsAltDown);
    }

    public void OnCanvasLeftMouseButtonDown(VecD pos) => Internals.ChangeController.LeftMouseButtonDownInlet(pos);
    public void OnCanvasMouseMove(VecD newPos)
    {
        DocumentsHandler.CoordinatesString = $"X: {(int)newPos.X} Y: {(int)newPos.Y}";
        Internals.ChangeController.MouseMoveInlet(newPos);
    }
    public void OnCanvasLeftMouseButtonUp() => Internals.ChangeController.LeftMouseButtonUpInlet();
    public void OnOpacitySliderDragStarted() => Internals.ChangeController.OpacitySliderDragStartedInlet();
    public void OnOpacitySliderDragged(float newValue) => Internals.ChangeController.OpacitySliderDraggedInlet(newValue);
    public void OnOpacitySliderDragEnded() => Internals.ChangeController.OpacitySliderDragEndedInlet();
    public void OnApplyTransform() => Internals.ChangeController.TransformAppliedInlet();
    public void OnSymmetryDragStarted(SymmetryAxisDirection dir) => Internals.ChangeController.SymmetryDragStartedInlet(dir);
    public void OnSymmetryDragged(SymmetryAxisDragInfo info) => Internals.ChangeController.SymmetryDraggedInlet(info);
    public void OnSymmetryDragEnded(SymmetryAxisDirection dir) => Internals.ChangeController.SymmetryDragEndedInlet(dir);
}
