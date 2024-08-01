using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Numerics;

namespace PixiEditor.Models.Handlers;

internal interface ITransformHandler : IHandler
{
    public void KeyModifiersInlet(bool argsIsShiftDown, bool argsIsCtrlDown, bool argsIsAltDown);
    public void ShowTransform(DocumentTransformMode transformMode, bool b, ShapeCorners shapeCorners, bool b1);
    public void HideTransform();
    public bool Undo();
    public bool Redo();
    public bool Nudge(VecD distance);
}
