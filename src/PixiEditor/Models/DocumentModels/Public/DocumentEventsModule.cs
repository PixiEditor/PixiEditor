using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Events;
using PixiEditor.Numerics;
using PixiEditor.ViewModels.SubViewModels.Document;
using PixiEditor.Views.UserControls.SymmetryOverlay;

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

    public void OnConvertedKeyDown(FilteredKeyEventArgs args)
    {
        Internals.ChangeController.ConvertedKeyDownInlet(args.Key);
        Document.TransformViewModel.ModifierKeysInlet(args.IsShiftDown, args.IsCtrlDown, args.IsAltDown);
    }
    public void OnConvertedKeyUp(FilteredKeyEventArgs args)
    {
        Internals.ChangeController.ConvertedKeyUpInlet(args.Key);
        Document.TransformViewModel.ModifierKeysInlet(args.IsShiftDown, args.IsCtrlDown, args.IsAltDown);
    }

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
    public void OnSymmetryDragStarted(SymmetryAxisDirection dir) => Internals.ChangeController.SymmetryDragStartedInlet(dir);
    public void OnSymmetryDragged(SymmetryAxisDragInfo info) => Internals.ChangeController.SymmetryDraggedInlet(info);
    public void OnSymmetryDragEnded(SymmetryAxisDirection dir) => Internals.ChangeController.SymmetryDragEndedInlet(dir);
}
