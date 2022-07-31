using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    public void OnKeyDown(Key args) => Internals.ChangeController.OnKeyDown(args);
    public void OnKeyUp(Key args) => Internals.ChangeController.OnKeyUp(args);

    public void OnCanvasLeftMouseButtonDown(VecD pos) => Internals.ChangeController.OnLeftMouseButtonDown(pos);
    public void OnCanvasMouseMove(VecD newPos)
    {
        Document.CoordinatesString = $"X: {(int)newPos.X} Y: {(int)newPos.Y}";
        Internals.ChangeController.OnMouseMove(newPos);
    }
    public void OnCanvasLeftMouseButtonUp() => Internals.ChangeController.OnLeftMouseButtonUp();
    public void OnOpacitySliderDragStarted() => Internals.ChangeController.OnOpacitySliderDragStarted();
    public void OnOpacitySliderDragged(float newValue) => Internals.ChangeController.OnOpacitySliderDragged(newValue);
    public void OnOpacitySliderDragEnded() => Internals.ChangeController.OnOpacitySliderDragEnded();
    public void OnApplyTransform() => Internals.ChangeController.OnTransformApplied();
}
